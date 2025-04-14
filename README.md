### Features

- CRUD operations for products
- JWT-based authentication
- FluentValidation with endpoint filters
- Exception handling
- Logging with Serilog
- Health checks with SQLite monitoring
- OpenAPI documentation with Swagger
- Minimal API endpoints

This is not a full list of features I would add to a new API, but I believe it meets the minimum set of requirements and shows in multiple ways the correct use of the SOLID principles.


### Installation

1. Clone the repository
2. Navigate to the project directory
3. Run `dotnet ef database update` (May need install ef tools)
4. Run `dotnet run`


### Testing

The project includes both unit tests and integration tests:


When running in development mode, navigate to /swagger for full API documentation.

I've added a get-token for testing purposes. 
When testing the API through Swagger:
1. You'll get a token from the `/get-token` endpoint
2. Click the "Authorize" button (padlock icon)
3. Enter your token with the "Bearer " prefix (e.g., "Bearer eyJhbGciOiJIUzI1...")
4. Click "Authorize"

### Overview
Basic overview of how the product API could fit in. 
I understand the diagram lacks event-driven architecture, 
which plays a big role in the reliability and performance of the application. 
However, I did not want to pretend I have a lot of knowledge in event-driven architecture.
<img width="743" alt="Image" src="https://github.com/user-attachments/assets/f7ac853e-5d20-4e8f-a553-43884479c649" />
