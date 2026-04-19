using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SchedulingService.BLL.Repositories;
using SchedulingService.BLL.Services;
using SchedulingService.Clients;
using SchedulingService.Data;
using SchedulingService.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ExternalServicesOptions>(
    builder.Configuration.GetSection("ExternalServices"));

builder.Services.AddHttpClient<IdentityClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ExternalServicesOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.IdentityUrl))
        throw new InvalidOperationException("ExternalServices:IdentityUrl configuration is required.");

    client.BaseAddress = new Uri(options.IdentityUrl);
});

builder.Services.AddHttpClient<IJobsServiceClient, JobsServiceClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ExternalServicesOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.JobsUrl))
        throw new InvalidOperationException("ExternalServices:JobsUrl configuration is required.");

    client.BaseAddress = new Uri(options.JobsUrl);
});

builder.Services.AddControllers();

builder.Services.AddDbContext<SchedulingDbContext>(options =>
{
    var cs = Environment.GetEnvironmentVariable("SCHEDULING_DB_CS");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("The SCHEDULING_DB_CS environment variable is not set.");

    options.UseSqlServer(cs);
});

builder.Services.AddScoped<StudentAvailabilityRepository>();
builder.Services.AddScoped<StudentAvailabilityService>();
builder.Services.AddScoped<ScheduledInterviewsRepository>();
builder.Services.AddScoped<InterviewSlotsRepository>();
builder.Services.AddScoped<InterviewSlotService>();
builder.Services.AddScoped<ScheduleInterviewsService>();

builder.Services.AddSingleton<IMapper>(_ =>
{
    var config = new MapperConfiguration(
        cfg =>
        {
            cfg.AddProfile<StudentAvailabilityProfile>();
            cfg.AddProfile<InterviewSlotProfile>();
            cfg.AddProfile<ScheduledInterviewProfile>();
        },
        NullLoggerFactory.Instance);
    config.AssertConfigurationIsValid();
    return config.CreateMapper();
});

var app = builder.Build();

app.MapControllers();

app.Run();
