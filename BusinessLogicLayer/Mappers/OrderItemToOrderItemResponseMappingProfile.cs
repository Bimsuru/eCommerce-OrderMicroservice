using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers;

public class OrderItemToOrderItemResponseMappingProfile : Profile
{
    public OrderItemToOrderItemResponseMappingProfile()
    {
        CreateMap<OrderItem, OrderItemResponse>()
       .ForMember(dest => dest.ProductID, option => option.MapFrom(src => src.ProductID))
       .ForMember(dest => dest.UnitPrice, option => option.MapFrom(src => src.UnitPrice))
       .ForMember(dest => dest.Quantity, option => option.MapFrom(src => src.Quantity))
       .ForMember(dest => dest.TotalPrice, option => option.MapFrom(src => src.TotalPrice));
    
    }
}
