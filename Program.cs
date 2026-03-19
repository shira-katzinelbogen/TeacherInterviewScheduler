using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SchedulingService.BLL.Repositories;
using SchedulingService.BLL.Services;
using SchedulingService.Data;
using SchedulingService.Mapping;

var builder = WebApplication.CreateBuilder(args);

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
<<<<<<< HEAD
            cfg.AddProfile<SchedulingService.BLL.Mapping.StudentAvailabilityDtoProfile>();
            cfg.AddProfile<SchedulingService.BLL.Mapping.InterviewSlotsDtoProfile>();
=======
            cfg.AddProfile<InterviewSlotProfile>();
            cfg.AddProfile<ScheduledInterviewProfile>();
>>>>>>> 1608011e318a92c93e4bef87d09e1c1d2757635b
        },
        NullLoggerFactory.Instance);
    config.AssertConfigurationIsValid();
    return config.CreateMapper();
});

var app = builder.Build();

app.MapControllers();

app.Run();
