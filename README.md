# Promptino

A RESTful API for managing AI-generated prompts and images, built with ASP.NET Core 8.0 following Clean Architecture principles.

## Overview

Promptino is a prompt management system that allows users to create, organize, and favorite AI prompts along with their associated generated images. The application provides role-based access control, JWT authentication, and comprehensive CRUD operations for both prompts and images.

## Architecture

The project follows Clean Architecture with clear separation of concerns:

- **Promptino.API**: Presentation layer with controllers and middleware
- **Promptino.Core**: Business logic, DTOs, services, and domain entities
- **Promptino.Infrastructure**: Data access, repositories, and external services
- **Promptino.Tests**: Unit and integration tests

## Key Features

- User authentication and authorization with JWT tokens
- Role-based access control (Admin/User roles)
- CRUD operations for prompts and images
- Image upload and storage management
- Favorite prompts functionality
- Search capabilities for prompts
- Many-to-many relationship between prompts and images (up to 6 images per prompt)
- Automatic role initialization on startup
- Comprehensive exception handling middleware

## Technology Stack

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- SQL Server
- AutoMapper for object mapping
- FluentValidation for input validation
- ASP.NET Core Identity for authentication
- JWT Bearer tokens
- XUnit for testing
- Moq for mocking

## Database Schema

The application manages four main entities:

- **ApplicationUser**: Extended IdentityUser with additional properties
- **Prompt**: Text prompts with title, description, and content
- **Image**: Image metadata with path and generation details
- **PromptImage**: Junction table for prompt-image relationships
- **FavoritePrompts**: User favorites tracking

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/auth/logout` - User logout
- `POST /api/auth/new-access-token` - Refresh access token

### Prompts
- `GET /api/prompts` - Get all prompts
- `GET /api/prompts/{id}` - Get prompt by ID
- `GET /api/prompts/search?keyword={keyword}` - Search prompts
- `GET /api/prompts/favorites/{userId}` - Get user's favorite prompts
- `POST /api/prompts/prompts/favorites` - Add prompt to favorites
- `DELETE /api/prompts/prompts/favorites/{userId}/{promptId}` - Remove from favorites

### Admin - Prompts
- `POST /api/admin/prompts` - Create prompt (Admin only)
- `PUT /api/admin/prompts` - Update prompt (Admin only)
- `DELETE /api/admin/prompts/{id}` - Delete prompt (Admin only)

### Admin - Images
- `POST /api/admin/CreateImage` - Upload image (Admin only)
- `PUT /api/admin/UpdateImage` - Update image (Admin only)
- `DELETE /api/admin/images/{id}` - Delete image (Admin only)
- `POST /api/admin/images/assign` - Assign image to prompt (Admin only)
- `DELETE /api/admin/images/assign` - Remove image from prompt (Admin only)

### Admin - Roles
- `GET /api/roles` - Get all roles (Admin only)
- `POST /api/roles/create` - Create role (Admin only)
- `POST /api/roles/add-user-to-role` - Assign role to user (Admin only)
- `POST /api/roles/remove-user-from-role` - Remove role from user (Admin only)
- `DELETE /api/roles/delete` - Delete role (Admin only)

## Configuration

Update `appsettings.json` with your configuration:

```json
{
  "ConnectionStrings": {
    "Default": "Your SQL Server connection string"
  },
  "JwtOptions": {
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "SecretKey": "your-secret-key",
    "ExpiryInMinutes": 10,
    "RefreshTokenExpiryInMinutes": 30
  }
}
```

## Getting Started

1. Clone the repository
2. Update the connection string in `appsettings.json`
3. Run the application - migrations will be applied automatically
4. Default admin account will be created:
   - Email: `promptinoadmin@gmail.com`
   - Password: `4sB4bId4RcH4M4N@123`

## Validation Rules

The application enforces comprehensive validation:

- Passwords require uppercase, lowercase, digit, and special character
- Prompts: 3-50 chars title, 10-150 chars description, 30-600 chars content
- Images: Valid extensions (.jpg, .jpeg, .png, .gif, .bmp, .webp, .svg)
- Email domains must be from recognized providers
- Phone numbers must be 11 digits in Iranian format

## Testing

The project includes comprehensive unit and integration tests covering:

- Repository operations
- Service layer business logic
- Prompt and image lifecycle management
- Validation scenarios
- Exception handling

Run tests using:
```bash
dotnet test
```

## Error Handling

Custom exceptions are handled through middleware with appropriate HTTP status codes:

- 400 Bad Request: Invalid input, validation errors
- 404 Not Found: Resource not found
- 409 Conflict: Resource already exists
- 500 Internal Server Error: Unexpected errors

## License

This project is for educational purposes.
