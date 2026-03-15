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

        modelBuilder.Entity<InterviewSlots>(entity =>
        {
            entity.ToTable("InterviewSlots");
            entity.HasKey(e => e.InterviewSlotID);
        });

        modelBuilder.Entity<ScheduledInterview>(entity =>
        {
            entity.ToTable("ScheduledInterview");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Comments)
                .HasMaxLength(500);

            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.InterviewSlotID);

            entity.HasOne(e => e.InterviewSlot)
                  .WithMany()
                  .HasForeignKey(e => e.InterviewSlotID);
        });
    }
}            
