using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SchedulingService.Data;

public class SchedulingDbContext : DbContext
{

    public SchedulingDbContext(){}
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options){}

    public DbSet<Entities.ScheduledInterview> ScheduledInterviews => Set<Entities.ScheduledInterview>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        // Called from the Environment Variable
        var connectionString = Environment.GetEnvironmentVariable("SCHEDULING_DB_CS");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The SCHEDULING_DB_CS environment variable is not set.");
        }
        optionsBuilder.UseSqlServer(connectionString);
    }
    // DbSet-s will come here...
}            
