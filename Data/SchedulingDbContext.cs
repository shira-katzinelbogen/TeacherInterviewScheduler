using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SchedulingService.Models;

namespace SchedulingService.Data;

public class SchedulingDbContext : DbContext
{

    public SchedulingDbContext(){}
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options){}
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

    public DbSet<InterviewSlots> InterviewSlots => Set<InterviewSlots>();
}            
