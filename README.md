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
- **User-owned prompts**: any authenticated user can create, update, and delete their own prompts; admins can manage all
- CRUD operations for prompts and images
- Image upload and storage management
- Save/bookmark prompts (replaces the old favorites system)
- Like / Dislike reactions, YouTube-style: one reaction per user per prompt, clicking again un-toggles, switching replaces
- Comments on prompts (public read, author or admin can delete)
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

The application manages these main entities:

- **ApplicationUser**: Extended IdentityUser with additional properties
- **Prompt**: Text prompts with title, description, content, and an owner (UserID)
- **Image**: Image metadata with path and generation details
- **PromptImage**: Junction table for prompt-image relationships
- **SavedPrompts**: User bookmarks of prompts (replaces FavoritePrompts)
- **Comment**: User comments on a prompt
- **PromptReaction**: One like/dislike per user per prompt (unique index on UserID+PromptID)

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/auth/logout` - User logout
- `POST /api/auth/new-access-token` - Refresh access token

### Prompts (Public)
- `GET /api/prompts` - Get all prompts (includes author, like/dislike/comment/save counts)
- `GET /api/prompts/{id}` - Get prompt by ID
- `GET /api/prompts/search?keyword={keyword}` - Search prompts

### Prompts (Owner or Admin)
- `POST /api/prompts` - Create a prompt owned by the current user (Auth required)
- `PUT /api/prompts` - Update own prompt; admins can update any (Auth required)
- `DELETE /api/prompts/{id}` - Delete own prompt; admins can delete any (Auth required)
- `GET /api/prompts/my` - Get the current user's prompts (Auth required)

### Saves (replaces favorites)
- `GET /api/prompts/saves` - Get the current user's saved prompts (Auth required)
- `POST /api/prompts/saves` - Save a prompt: body `{ "promptID": "..." }` (Auth required)
- `DELETE /api/prompts/saves/{promptId}` - Remove a prompt from saves (Auth required)
- `GET /api/prompts/saves/count/{promptId}` - How many users saved a prompt (public)
- `GET /api/prompts/saves/{promptId}/status` - Has the current user saved this prompt? (Auth required)

### Reactions (Like / Dislike)
- `PUT /api/prompts/{promptId}/reaction` - Set reaction: body `{ "type": 1 }` (1 = Like, 2 = Dislike); clicking the same again un-toggles (Auth required)
- `DELETE /api/prompts/{promptId}/reaction` - Remove own reaction (Auth required)
- `GET /api/prompts/{promptId}/reaction/state` - Get counts + caller's current reaction (public)

### Comments
- `GET /api/prompts/{promptId}/comments` - List comments on a prompt (public)
- `POST /api/prompts/{promptId}/comments` - Add a comment (Auth required): body `{ "content": "..." }`
- `DELETE /api/prompts/{promptId}/comments/{commentId}` - Delete own comment; admins can delete any (Auth required)

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
- Comments: 2-500 chars content
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
- 403 Forbidden: You don't own this prompt/comment
- 404 Not Found: Resource not found
- 409 Conflict: Resource already exists (e.g., prompt already saved, duplicate reaction)
- 500 Internal Server Error: Unexpected errors

> **Breaking change:** the old `/api/prompts/favorites*` endpoints have been removed. Use the `/saves` and `/{promptId}/reaction` endpoints instead. Applying this migration also wipes existing prompt data so every prompt gets an owner going forward.

## License

This project is for educational purposes.
