using SchedulingService.Clients.Models;

namespace SchedulingService.Clients;

/// <summary>
/// Outbound HTTP calls to the Jobs microservice. Base address is configured when <see cref="HttpClient"/> is registered.
/// Pass <paramref name="bearerToken"/> (without the "Bearer " prefix) to forward the caller's JWT downstream.
/// </summary>
public interface IJobsServiceClient
{
    Task<JobDto?> GetJobByIdAsync(int jobId, string? bearerToken = null, CancellationToken cancellationToken = default);

    Task<PaginatedListDto<JobDto>?> GetPublicJobsAsync(
        IReadOnlyList<string>? statuses = null,
        bool newestFirst = true,
        int pageNumber = 1,
        int pageSize = 10,
        string? bearerToken = null,
        CancellationToken cancellationToken = default);
}
