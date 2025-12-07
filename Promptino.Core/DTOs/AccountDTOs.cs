namespace Promptino.Core.DTOs;

public record AuthResponse
(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    bool IsLockedOut,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    string Token,
    string RefreshToken,
    DateTime RefreshTokenExpiry
)
{
    public AuthResponse() : this(
        default, default, default,
        default, default, default, 
        default, default, default, default, default)
    { }
};

public record RegisterRequest
(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber
)
{
    public RegisterRequest() : this(
        default!, default!, default!,
        default!, default!)
    { }
};

public record LoginRequest
(
    string Email,
    string Password,
    string ConfirmPassword
)
{
    public LoginRequest() : this(default, default,default)
    { }
}

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
)
{
    public RefreshTokenRequest() : this(default!, default!)
    { }
}