
using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services;

public class OrderService : IOrderService
{
    private readonly IMapper _mapper;
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly ValidationService _validationService;
    private readonly UserMicroserviceClient _userMicroserviceClient;
    private readonly ProductMicroserviceClient _productMicroserviceClient;
    public OrderService(IMapper mapper, IOrderRepository orderRepository, IValidator<OrderAddRequest> orderAddRequestValidator,
                        IValidator<OrderItemAddRequest> orderItemAddRequestValidator, ValidationService validationService,
                        IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
                        UserMicroserviceClient userMicroserviceClient, ProductMicroserviceClient productMicroserviceClient)
    {
        _mapper = mapper;
        _orderRepository = orderRepository;
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _validationService = validationService;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _userMicroserviceClient = userMicroserviceClient;
        _productMicroserviceClient = productMicroserviceClient;
    }
    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        // OrderAddRequest validation using validation service class method --> ModelValidationAsync()
        await _validationService.ModelValidationAsync(_orderAddRequestValidator, orderAddRequest, nameof(orderAddRequest));

        // OrderItems validation using validation service class method --> CollectionModelValidationAsync()
        await _validationService.CollectionModelVaidationAsync(_orderItemAddRequestValidator, orderAddRequest.OrderItems, nameof(orderAddRequest.OrderItems));

        var order = _mapper.Map<Order>(orderAddRequest);

        // GetUserAndProductsByIds using user and product microservices
        var userAndProductsRes = await GetUserAndProductsByIds(order);


        // Calculation
        order = Calculation(order);

        // Invoke the orderRepository --> AddOrder method
        var addedOrder = await _orderRepository.AddOrder(order);


        if (addedOrder != null)
        {
            // Invoke creation of orderResponse
            var orderResponse = CreateOrderResponse(addedOrder, userAndProductsRes.user, userAndProductsRes.products);
            return orderResponse;
        }
        else
            return null;
    }

    public async Task<bool> DeleteOrder(Guid orderId)
    {
        bool isDeleted = await _orderRepository.DeleteOrder(orderId);
        return isDeleted;
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        var order = await _orderRepository.GetOrderByCondition(filter);

        if (order != null)
        {
            var userAndProductsRes = await GetUserAndProductsByIds(order!);
            var orderResponse = CreateOrderResponse(order!, userAndProductsRes.user, userAndProductsRes.products);

            return orderResponse;
        }

        else
            return null;
    }
    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        var orders = await _orderRepository.GetOrdersByCondition(filter);

        List<OrderResponse> orderResponses = new List<OrderResponse>();

        if (orders != null)
        {
            foreach (var order in orders)
            {
                // get the products and users objects from the dependent services
                var userAndProductsRes = await GetUserAndProductsByIds(order!);

                var orderResponse = CreateOrderResponse(order!, userAndProductsRes.user, userAndProductsRes.products);

                orderResponses.Add(orderResponse);
            }
            return orderResponses.ToList()!;
        }

        else
            return null;
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        // orderUpdateRequest validation using validation service class method --> ModelValidationAsync()
        await _validationService.ModelValidationAsync(_orderUpdateRequestValidator, orderUpdateRequest, nameof(orderUpdateRequest));

        // orderUpdateRequest in OrderItems validation using validation service class method --> CollectionModelValidationAsync()
        await _validationService.CollectionModelVaidationAsync(_orderItemUpdateRequestValidator, orderUpdateRequest.OrderItems, nameof(orderUpdateRequest.OrderItems));

        // Mapped update model into order model
        var order = _mapper.Map<Order>(orderUpdateRequest);

        // GetUserAndProductsByIds using user and product microservices and this res have products and users objects
        var userAndProductsRes = await GetUserAndProductsByIds(order);

        // Create filter
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, order.OrderID);

        // check exitting order
        var exittingOrder = await _orderRepository.GetOrderByCondition(filter);
        if (exittingOrder == null)
            return null;

        var updateOrderInput = _mapper.Map(order, exittingOrder);

        // Calculation
        updateOrderInput = Calculation(updateOrderInput);

        // Add uniq id value into _id
        updateOrderInput._id = exittingOrder._id;


        // Invoke
        var updatedOrderRes = await _orderRepository.UpdateOrder(updateOrderInput);

        if (updatedOrderRes != null)
        {
            // Invoke creation of orderResponse
            var orderResponse = CreateOrderResponse(updatedOrderRes, userAndProductsRes.user, userAndProductsRes.products);
            return orderResponse;
        }

        return null;
    }

    private Order Calculation(Order order)
    {
        foreach (var orderItem in order.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }
        order.TotalBill = order.OrderItems.Sum(temp => temp.TotalPrice);

        return order;
    }

    private async Task<(UserResponse user, List<ProductResponse> products)> GetUserAndProductsByIds(Order order)
    {
        // Check user id is valid or not
        // invoke users microservice using httpclient and req this microservice 
        var user = await _userMicroserviceClient.GetUserByIdAsync(order.UserID);

        if (user == null)
        {
            throw new ArgumentException($"Invalid user id : {order.UserID}");

        }

        // Check productid using product microservice
        List<ProductResponse> products = new List<ProductResponse>();

        foreach (var item in order.OrderItems)
        {
            var product = await _productMicroserviceClient.GetProductByIdAsync(item.ProductID);

            if (product == null)
                throw new ArgumentException($"Invalid product id : {item.ProductID}");

            products.Add(product);
        }

        return (user, products);
    }

    private OrderResponse CreateOrderResponse(Order order, UserResponse user, List<ProductResponse> products)
    {

        var orderResponse = _mapper.Map<OrderResponse>(order);

        _mapper.Map<UserResponse, OrderResponse>(user, orderResponse);

        foreach (var orderItemRes in orderResponse.OrderItems)
        {
            var productRes = products.Where(temp => temp.ProductID == orderItemRes.ProductID).FirstOrDefault();

            if (productRes == null)
                continue;

            _mapper.Map<ProductResponse, OrderItemResponse>(productRes, orderItemRes);
        }

        return orderResponse;
    }



}
