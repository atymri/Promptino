using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Core.Services.PromptImageServices;

public class PromptImageRepository : IPromptImageRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPromptRepository _promptRepository;
    private readonly IImageRepository _imageRepository;

    public PromptImageRepository(
        ApplicationDbContext context,
        IMapper mapper,
        IPromptRepository promptRepository,
        IImageRepository imageRepository)
    {
        _context = context;
        _mapper = mapper;
        _promptRepository = promptRepository;
        _imageRepository = imageRepository;
    }

    public async Task<PromptResponse> CreatePromptWithImagesAsync(PromptAddRequest promptRequest)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var prompt = _mapper.Map<Prompt>(promptRequest);
            var createdPrompt = await _promptRepository.AddPromptAsync(prompt);

            if (createdPrompt == null)
                throw new InvalidOperationException("Failed to create prompt");

            if (promptRequest.Images != null && promptRequest.Images.Any())
            {
                foreach (var imageRequest in promptRequest.Images)
                {
                    var image = _mapper.Map<Image>(imageRequest);
                    var createdImage = await _imageRepository.AddImageAsync(image);

                    if (createdImage == null)
                        throw new InvalidOperationException("Failed to create image");

                    var success = await _imageRepository.AddImageToPromptAsync(
                        createdPrompt.ID,
                        createdImage.ID);

                    if (!success)
                        throw new InvalidOperationException($"Failed to link image {createdImage.ID} to prompt {createdPrompt.ID}");
                }
            }

            await transaction.CommitAsync();

            var fullPrompt = await _promptRepository.GetPromptByConditionAsync(
                p => p.ID == createdPrompt.ID); 

            return _mapper.Map<PromptResponse>(fullPrompt);
        }
        catch
        {
            throw;
        }
    }
}