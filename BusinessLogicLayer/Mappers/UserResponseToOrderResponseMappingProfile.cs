using AutoMapper;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Mappers;

public class UserResponseToOrderResponseMappingProfile : Profile
{
    public UserResponseToOrderResponseMappingProfile()
    {
        CreateMap<UserResponse, OrderResponse>()
      .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
      .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
      .ForMember(dest => dest.PersonName, opt => opt.MapFrom(src => src.PersonName));
    }
}
