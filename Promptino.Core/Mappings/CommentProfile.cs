using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

namespace Promptino.Core.Mappings;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<CommentAddRequest, Comment>()
            .ForMember(dest => dest.ID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.PromptID, opt => opt.MapFrom(src => src.PromptID))
            .ForMember(dest => dest.ParentCommentID, opt => opt.MapFrom(src => src.ParentCommentID))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Prompt, opt => opt.Ignore())
            .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
            .ForMember(dest => dest.Replies, opt => opt.Ignore())
            .ForMember(dest => dest.Likes, opt => opt.Ignore());

        // flat members only; replies are grouped/enriched in CommentGetterService
        CreateMap<Comment, CommentResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserID))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => PromptProfile.ResolveAuthorName(src.User)))
            .ForMember(dest => dest.PromptId, opt => opt.MapFrom(src => src.PromptID))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.ParentCommentID, opt => opt.MapFrom(src => src.ParentCommentID))
            .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes != null ? src.Likes.Count : 0))
            .ForMember(dest => dest.Replies, opt => opt.Ignore());
    }
}
