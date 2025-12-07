using Microsoft.AspNetCore.Http;

namespace Promptino.Core.DTOs;

public record ImageAddRequest(
    string Title,
    string Path,
    string GeneratedWith
)
{
    public ImageAddRequest() : this(default,default, default)
    { }
};

public record ImageUpdateRequest(
    Guid Id,
    string Title,
    string Path,
    IFormFile? file,
    string GeneratedWith
)
{
    public ImageUpdateRequest() : this(default, default, default, default, default)
    { }
};

public record ImageResponse(
    Guid Id,
    string Title,
    string Path,
    string GeneratedWith
)
{
    public ImageResponse() : this(default, default, default, default)
    { }
};
