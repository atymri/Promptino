using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

namespace Promptino.Core.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryAddRequest, Category>()
            .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dst => dst.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dst => dst.ID, opt => opt.Ignore())
            .ForMember(dst => dst.CreatedAt, opt => opt.Ignore())
            .ForMember(dst => dst.LastUpdatedAt, opt => opt.Ignore());

        CreateMap<CategoryUpdateRequest, Category>()
            .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dst => dst.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dst => dst.ID, opt => opt.MapFrom(src => src.CategoryID))
            .ForMember(dst => dst.CreatedAt, opt => opt.Ignore())
            .ForMember(dst => dst.LastUpdatedAt, opt => opt.Ignore());

        CreateMap<Category, CategoryResponse>()
            .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dst => dst.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dst => dst.CategoryID, opt => opt.MapFrom(src => src.ID));

    }
}
