using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promptino.Core.Mappings;

public class PromptProfile : Profile
{
    public PromptProfile()
    {
        CreateMap<PromptAddRequest, Prompt>()
            .ForMember(dest => dest.ID, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.PromptImages, opt => opt.MapFrom(src => src.Images))
            .ForMember(dest => dest.FavoritePrompts, opt => opt.Ignore());

        CreateMap<PromptUpdateRequest, Prompt>()
            .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.PromptImages, opt => opt.Ignore())
            .ForMember(dest => dest.FavoritePrompts, opt => opt.Ignore());

        CreateMap<Prompt, PromptResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.PromptImages));
    }
}
