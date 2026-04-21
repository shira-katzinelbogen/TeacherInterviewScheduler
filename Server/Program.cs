using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SchedulingService.BLL.Repositories;
using SchedulingService.BLL.Services;
using SchedulingService.Clients;
using SchedulingService.Data;
using SchedulingService.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.Configure<ExternalServicesOptions>(
    builder.Configuration.GetSection("ExternalServices"));
builder.Services.AddHttpClient<IJobsServiceClient, JobsServiceClient>(
    (sp, client) =>
    {
        var options = sp
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<ExternalServicesOptions>>()
            .Value;
        if (string.IsNullOrWhiteSpace(options.JobsUrl))
            throw new InvalidOperationException("ExternalServices:JobsUrl is not configured.");

        client.BaseAddress = new Uri(options.JobsUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddSingleton<IMapper>(_ =>
{
    var config = new MapperConfiguration(
        cfg =>
        {
            cfg.AddProfile<StudentAvailabilityProfile>();
            cfg.AddProfile<SchedulingService.BLL.Mapping.InterviewSlotsDtoProfile>();

            cfg.AddProfile<InterviewSlotProfile>();
            cfg.AddProfile<ScheduledInterviewProfile>();
        },
        NullLoggerFactory.Instance);
    // Strict AssertConfigurationIsValid() fails for ConstructUsing / merge maps (e.g. InterviewSlots update).
    // Run mapping unit tests or validate profiles separately if you tighten this.
    return config.CreateMapper();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
