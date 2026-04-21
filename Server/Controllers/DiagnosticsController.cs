using Microsoft.AspNetCore.Mvc;
using SchedulingService.Clients;
using SchedulingService.Clients.Models;

namespace SchedulingService.Controllers;

[ApiController]
[Route("api/diagnostics")]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly IJobsServiceClient _jobsServiceClient;

    public DiagnosticsController(IJobsServiceClient jobsServiceClient)
    {
        _jobsServiceClient = jobsServiceClient;
    }

    /// <summary>
    /// Verifies outbound connectivity from SchedulingService to JobService by fetching a job.
    /// Forwards the caller's Authorization: Bearer token to JobService.
    /// </summary>
    [HttpGet("jobs/{jobId:int}")]
    public async Task<IActionResult> CheckJobsServiceConnection(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        if (jobId <= 0)
            return BadRequest("jobId must be greater than 0.");

        var authHeader = Request.Headers.Authorization.ToString();
        var bearerToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..]
            : null;

        try
        {
            var job = await _jobsServiceClient.GetJobByIdAsync(jobId, bearerToken, cancellationToken);
            if (job is null)
                return NotFound("Job not found in JobService.");

            return Ok(job);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            {
                return Ok(new
                {
                    reachable = true,
                    authorized = false,
                    downstreamStatus = (int)ex.StatusCode.Value,
                    message = "JobService is reachable but rejected the request (no or invalid token forwarded)."
                });
            }

            return StatusCode(
                StatusCodes.Status502BadGateway,
                $"Failed to reach JobService: {ex.Message}");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status504GatewayTimeout,
                "Request to JobService timed out.");
        }
    }
}
