using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

namespace Promptino.Core.Mappings;

public class PromptReactionProfile : Profile
{
    public PromptReactionProfile()
    {
        CreateMap<PromptReaction, ReactionStateResponse>()
            .ConstructUsing((src, ctx) =>
            {
                var counts = src.Prompt?.Reactions;
                return new ReactionStateResponse(
                    counts?.Count(r => r.Type == ReactionType.Like) ?? 0,
                    counts?.Count(r => r.Type == ReactionType.Dislike) ?? 0,
                    src.Type);
            });
    }
}
