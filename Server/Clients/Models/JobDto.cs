namespace SchedulingService.Clients.Models;

/// <summary>
/// Response shape for job JSON from the Jobs microservice. Property names match their contract
/// (see Team 4 <c>JobDto</c>) so <see cref="System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync"/> works.
/// </summary>
public sealed class JobDto
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Experience { get; set; }
    public string Requirements { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? JobWebsiteUrl { get; set; }
    public string? JobImageUrl { get; set; }
    public bool IsRemote { get; set; }
    public bool IsPrivate { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public int? SalaryMin { get; set; }
    public int? SalaryMax { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}

/// <summary>
/// Tag entry on <see cref="JobDto"/>; names align with Team 4 <c>TagDto</c>.
/// </summary>
public sealed class TagDto
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
}
