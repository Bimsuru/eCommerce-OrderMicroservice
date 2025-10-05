using AutoMapper;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Mappers;

public class ProductResponseToOrderItemResponseMappingProfile : Profile
{
    public ProductResponseToOrderItemResponseMappingProfile() 
    {
        CreateMap<ProductResponse, OrderItemResponse>()
       .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID))
       .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
       .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
       .ForMember(dest => dest.UnitPrice, opt => opt.Ignore());
    }
}
