using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers;

public class OrderAddRequestToOrderMappingProfile : Profile
{
    public OrderAddRequestToOrderMappingProfile()
    {
        CreateMap<OrderAddRequest, Order>()
        .ForMember(dest => dest.UserID, option => option.MapFrom(src => src.UserID))
        .ForMember(dest => dest.OrderDate, option => option.MapFrom(src => src.OrderDate))
        .ForMember(dest => dest.OrderItems, option => option.MapFrom(src => src.OrderItems))
        .ForMember(dest => dest.OrderID, option => option.Ignore())
        .ForMember(dest => dest.TotalBill, option => option.Ignore())
        .ForMember(dest => dest._id, option => option.Ignore());
    }
}
