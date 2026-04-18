namespace SchedulingService.Clients.Models;

/// <summary>
/// Paged payload returned by the Jobs service for list endpoints (e.g. <c>GET api/jobs</c>).
/// </summary>
public sealed class PaginatedListDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
