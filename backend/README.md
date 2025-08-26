# VoxMed API - Backend

A .NET 8.0 REST API for the VoxMed application.

## Technologies

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core (ready to be configured)
- Swagger/OpenAPI documentation

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension

### Running the Application

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Or for development with hot reload
dotnet watch run
```

The API will be available at:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:7000/swagger`

## Project Structure

```
backend/
├── Controllers/        # API Controllers
├── Properties/         # Launch settings
├── appsettings.json   # Configuration
├── Program.cs         # Application entry point
└── VoxMedApi.csproj   # Project file
```

## Development

The project is configured with:
- Controllers-based routing
- CORS enabled for development
- Swagger/OpenAPI documentation
- Development exception handling

### Next Steps

1. Configure Entity Framework Core for database access
2. Add authentication and authorization
3. Implement medical domain models and controllers
4. Add logging and monitoring
5. Configure production settings
