namespace SchedulingService.Clients;

public sealed class IdentityClient
{
    private readonly HttpClient _httpClient;

    public IdentityClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    // Add identity-related HTTP methods here.
}
