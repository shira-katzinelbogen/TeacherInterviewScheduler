using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using SchedulingService.Clients.Models;

namespace SchedulingService.Clients;

/// <summary>
/// Typed client for the Jobs API. Uses relative URIs only; <see cref="HttpClient.BaseAddress"/> must be set by registration.
/// </summary>
public sealed class JobsServiceClient : IJobsServiceClient
{
    private readonly HttpClient _httpClient;

    public JobsServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public Task<JobDto?> GetJobByIdAsync(int jobId, string? bearerToken = null, CancellationToken cancellationToken = default)
    {
        if (jobId <= 0)
            throw new ArgumentOutOfRangeException(nameof(jobId));

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/jobs/{jobId}");
        ApplyToken(request, bearerToken);
        return SendAndDeserializeAsync<JobDto>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaginatedListDto<JobDto>?> GetPublicJobsAsync(
        IReadOnlyList<string>? statuses = null,
        bool newestFirst = true,
        int pageNumber = 1,
        int pageSize = 10,
        string? bearerToken = null,
        CancellationToken cancellationToken = default)
    {
        var path = new StringBuilder("api/jobs?");
        path.Append("newestFirst=").Append(newestFirst ? "true" : "false");
        path.Append("&pageNumber=").Append(pageNumber);
        path.Append("&pageSize=").Append(pageSize);

        if (statuses is not null)
        {
            foreach (var status in statuses)
            {
                path.Append("&statuses=");
                path.Append(Uri.EscapeDataString(status));
            }
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path.ToString());
        ApplyToken(request, bearerToken);
        return await SendAndDeserializeAsync<PaginatedListDto<JobDto>>(request, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void ApplyToken(HttpRequestMessage request, string? bearerToken)
    {
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    private async Task<T?> SendAndDeserializeAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
