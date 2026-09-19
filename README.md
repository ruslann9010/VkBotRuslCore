
# VkCoreRuslBot (GigaChat API Integration)

A C# .NET integration service for interacting with the Sberbank **GigaChat API**. This service handles secure OAuth2 authentication, automatic token caching with expiration management, and token refresh synchronization.

## Features

- **Automated Authentication**: Connects to the Sberbank OAuth gateway using `Basic` credentials.
- **Efficient Token Caching**: Tokens are cached in-memory and automatically refreshed only upon expiration (30-minute lifespan).
- **Thread-Safe Architecture**: Employs `SemaphoreSlim` to guarantee safe token updates under concurrent environments and strictly respect API rate limits.
- **Resource Optimized**: Avoids socket exhaustion by utilizing a single, static `HttpClient` instance.

## Project Structure

- `AiService.cs` — The core integration service handling API authorization and token lifetimes.
- `appsettings.json` — Local configuration file containing sensitive API keys (**Excluded from Git**).
- `appsettings.Example.json` — A template demonstrating the required configuration schema.

## Getting Started

### Prerequisites
- .NET 8.0 SDK or higher
- GigaChat API Client ID and Client Secret (obtained from SberDevices space)

### Configuration

Since `appsettings.json` contains private credentials, it is ignored by Git. To set up your local environment:

1. Duplicate the `appsettings.Example.json` file.
2. Rename the copy to `appsettings.json`.
3. Populate it with your authentic **Authorization Key**:

```json
{
  "GigaChat": {
    "AuthorizationKey": "YOUR_ACTUAL_BASE64_KEY"
  }
}
```

### Installation & Run

1. Clone the repository (if hosted):
   ```bash
   git clone <repository-url>
   cd VkCoreRuslBot
   ```
2. Restore dependencies and build the solution:
   ```bash
   dotnet build
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

## Usage Example

```csharp
// Initialize the service with your authorization key
var aiService = new AiService("YOUR_BASIC_AUTH_KEY");

// Retrieve a valid token (fetches from API or returns cached instance)
string accessToken = await aiService.GetAccessTokenAsync();
Console.WriteLine($"Token retrieved successfully.");
```

## Security Note

- **Never** commit your `appsettings.json` file. 
- Ensure that the SSL certificate validation fallback (`ServerCertificateCustomValidationCallback`) is removed or restricted to trusted Russian Ministry of Digital Development (Минцифры) root certificates before deploying to production environments.
