using System;
using Microsoft.EntityFrameworkCore;
using SchedulingService.Models;

namespace SchedulingService.Data;

public class SchedulingDbContext : DbContext
{
    public SchedulingDbContext(){}
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options){}

    public DbSet<InterviewSlots> InterviewSlots => Set<InterviewSlots>();
    public DbSet<StudentAvailability> StudentAvailabilities => Set<StudentAvailability>();
    public DbSet<ScheduledInterview> ScheduledInterviews => Set<ScheduledInterview>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        var connectionString = Environment.GetEnvironmentVariable("SCHEDULING_DB_CS");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The SCHEDULING_DB_CS environment variable is not set.");
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentAvailability>(entity =>
        {
            entity.ToTable("StudentAvailability");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ReasonStudent)
                .HasMaxLength(500);

            entity.HasIndex(e => e.StudentId);
        });

        modelBuilder.Entity<InterviewSlots>(e =>
        {
            e.ToTable("InterviewSlots");
            e.HasKey(x => x.InterviewSlotID);
        });
    }
}            
