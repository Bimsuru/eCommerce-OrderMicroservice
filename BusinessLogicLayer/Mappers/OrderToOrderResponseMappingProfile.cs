using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers;

public class OrderToOrderResponseMappingProfile : Profile
{
    public OrderToOrderResponseMappingProfile()
    {
        CreateMap<Order, OrderResponse>()
       .ForMember(dest => dest.UserID, option => option.MapFrom(src => src.UserID))
       .ForMember(dest => dest.OrderDate, option => option.MapFrom(src => src.OrderDate))
       .ForMember(dest => dest.OrderID, option => option.MapFrom(src => src.OrderID))
       .ForMember(dest => dest.TotalBill, option => option.MapFrom(src => src.TotalBill))
       .ForMember(dest => dest.OrderItems, option => option.MapFrom(src => src.OrderItems));
    }
}
