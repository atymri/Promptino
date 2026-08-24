using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

namespace Promptino.Core.Mappings;

public class SavedPromptsProfile : Profile
{
    public SavedPromptsProfile()
    {
        CreateMap<SavedPromptAddRequest, SavedPrompt>()
            .ForMember(dest => dest.ID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.PromptID, opt => opt.MapFrom(src => src.PromptID))
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Prompt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        CreateMap<SavedPrompt, SavedPromptResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserID))
            .ForMember(dest => dest.PromptId, opt => opt.MapFrom(src => src.PromptID))
            .ForMember(dest => dest.PromptTitle, opt => opt.MapFrom(src => src.Prompt != null ? src.Prompt.Title : string.Empty))
            .ForMember(dest => dest.PromptDescription, opt => opt.MapFrom(src => src.Prompt != null ? src.Prompt.Description : string.Empty))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<SavedPrompt, SavedWithDetailsResponse>()
            .ForMember(dest => dest.SavedId, opt => opt.MapFrom(src => src.ID))
            .ForMember(dest => dest.Prompt, opt => opt.MapFrom(src => src.Prompt))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}
