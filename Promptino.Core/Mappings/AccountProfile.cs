using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

namespace Promptino.Core.Mappings;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<ApplicationUser, AuthResponse>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dst => dst.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dst => dst.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dst => dst.Token, opt => opt.Ignore())
            .ForMember(dst => dst.LastLoginAt, opt => opt.Ignore())
            .ForMember(dst => dst.IsLockedOut, opt => opt.Ignore());


        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dst => dst.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dst => dst.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dst => dst.RefreshToken, opt => opt.Ignore())
            .ForMember(dst => dst.RefreshTokenExpiration, opt => opt.Ignore()) ;
    }
}
