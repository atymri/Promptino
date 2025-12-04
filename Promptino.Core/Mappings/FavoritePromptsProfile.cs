using AutoMapper;
using Promptino.Core.DTOs;
using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Mappings;

public class FavoritePromptsProfile : Profile
{
    public FavoritePromptsProfile()
    {
        CreateMap<FavoritePromptAddRequest, FavoritePrompts>()
            .ForMember(dest => dest.ID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
            .ForMember(dest => dest.PromptID, opt => opt.MapFrom(src => src.PromptID))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
     
        CreateMap<FavoritePrompts, FavoritePromptResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserID))
            .ForMember(dest => dest.PromptId, opt => opt.MapFrom(src => src.PromptID))
            .ForMember(dest => dest.PromptTitle, opt => opt.MapFrom(src => src.Prompt!.Title))
            .ForMember(dest => dest.PromptDescription, opt => opt.MapFrom(src => src.Prompt!.Description))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
        
        CreateMap<FavoritePrompts, FavoriteWithDetailsResponse>()
            .ForMember(dest => dest.FavoriteId, opt => opt.MapFrom(src => src.ID))
            .ForMember(dest => dest.Prompt, opt => opt.MapFrom(src => src.Prompt))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}