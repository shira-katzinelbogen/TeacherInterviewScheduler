using SchedulingService.Clients.Models;

namespace SchedulingService.Clients;

/// <summary>
/// Outbound HTTP calls to the Jobs microservice. Base address is configured when <see cref="HttpClient"/> is registered.
/// </summary>
public interface IJobsServiceClient
{
    Task<JobDto?> GetJobByIdAsync(int jobId, CancellationToken cancellationToken = default);

    Task<PaginatedListDto<JobDto>?> GetPublicJobsAsync(
        IReadOnlyList<string>? statuses = null,
        bool newestFirst = true,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
