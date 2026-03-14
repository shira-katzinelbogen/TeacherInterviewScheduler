using System;
using Microsoft.EntityFrameworkCore;
using SchedulingService.Models;

namespace SchedulingService.Data;

public class SchedulingDbContext : DbContext
{
    public DbSet<StudentAvailability> StudentAvailabilities => Set<StudentAvailability>();
    public DbSet<InterviewSlots> InterviewSlots => Set<InterviewSlots>();

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentAvailability>(e =>
        {
            e.ToTable("StudentAvailability");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<InterviewSlots>(e =>
        {
            e.ToTable("InterviewSlots");
            e.HasKey(x => x.InterviewSlotID);
        });
    }
}            
