using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

namespace Promptino.Core.Mappings;

public class ImageProfile : Profile
{
    public ImageProfile()
    {
        CreateMap<ImageAddRequest, Image>()
            .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dst => dst.GeneratedWith, opt => opt.MapFrom(src => src.GeneratedWith))
            .ForMember(dst => dst.Path, opt => opt.MapFrom(src => src.Path));

        CreateMap<ImageUpdateRequest, Image>()
            .ForMember(dst => dst.ID, opt => opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dst => dst.GeneratedWith, opt => opt.MapFrom(src => src.GeneratedWith))
            .ForMember(dst => dst.Path, opt => opt.MapFrom(src => src.Path));

        CreateMap<Image, ImageResponse>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.ID))
            .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dst => dst.GeneratedWith, opt => opt.MapFrom(src => src.GeneratedWith))
            .ForMember(dst => dst.Path, opt => opt.MapFrom(src => src.Path));

    }
}

