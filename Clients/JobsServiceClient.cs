using System.Net.Http.Json;
using System.Text;
using SchedulingService.Clients.Models;

namespace SchedulingService.Clients;

/// <summary>
/// Typed client for the Jobs API. Uses relative URIs only; <see cref="HttpClient.BaseAddress"/> must be set by registration (see team config task).
/// </summary>
public sealed class JobsServiceClient : IJobsServiceClient
{
    private readonly HttpClient _httpClient;

    public JobsServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public Task<JobDto?> GetJobByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        if (jobId <= 0)
            throw new ArgumentOutOfRangeException(nameof(jobId));

        return _httpClient.GetFromJsonAsync<JobDto>($"api/jobs/{jobId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaginatedListDto<JobDto>?> GetPublicJobsAsync(
        IReadOnlyList<string>? statuses = null,
        bool newestFirst = true,
        int pageNumber = 1,
        int pageSize = 10,
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

        return await _httpClient.GetFromJsonAsync<PaginatedListDto<JobDto>>(path.ToString(), cancellationToken)
            .ConfigureAwait(false);
    }
}
