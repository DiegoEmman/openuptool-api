using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using System.Text.Json;

namespace OpenUpTool.Infrastructure.Data;

public class OpenUpToolDbContext : DbContext
{
    public OpenUpToolDbContext(DbContextOptions<OpenUpToolDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Phase> Phases => Set<Phase>();
    public DbSet<ProjectPlan> ProjectPlans => Set<ProjectPlan>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Iteration> Iterations => Set<Iteration>();
    public DbSet<ArtifactType> ArtifactTypes => Set<ArtifactType>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Projects
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Identifier).HasColumnName("identifier").IsRequired().HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnName("start_date").HasColumnType("timestamp");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.Owner).HasColumnName("owner").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Tags).HasColumnName("tags")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("jsonb");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").ValueGeneratedOnAddOrUpdate();

            entity.HasOne(e => e.Plan)
                .WithOne(p => p.Project)
                .HasForeignKey<ProjectPlan>(p => p.ProjectId);

            entity.HasMany(e => e.Phases)
                .WithOne(p => p.Project)
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Iterations)
                .WithOne(i => i.Project)
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Artifacts)
                .WithOne(a => a.Project)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Phases
        modelBuilder.Entity<Phase>(entity =>
        {
            entity.ToTable("phases");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.PhaseCode).HasColumnName("phase_code").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.ActualStart).HasColumnName("actual_start");
            entity.Property(e => e.ActualEnd).HasColumnName("actual_end");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => new { e.ProjectId, e.PhaseCode }).IsUnique();
        });

        // ProjectPlans
        modelBuilder.Entity<ProjectPlan>(entity =>
        {
            entity.ToTable("project_plans");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Objectives).HasColumnName("objectives").IsRequired();
            entity.Property(e => e.Scope).HasColumnName("scope").IsRequired();
            entity.Property(e => e.InitialSchedule).HasColumnName("initial_schedule")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<PhaseScheduleItem>>(v, (JsonSerializerOptions?)null) ?? new List<PhaseScheduleItem>()
                )
                .HasColumnType("jsonb");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasMany(e => e.Milestones)
                .WithOne(m => m.Plan)
                .HasForeignKey(m => m.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Milestones
        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.ToTable("milestones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // Iterations
        modelBuilder.Entity<Iteration>(entity =>
        {
            entity.ToTable("iterations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Objective).HasColumnName("objective");
            entity.Property(e => e.Phase).HasColumnName("phase").IsRequired().HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // ArtifactTypes
        modelBuilder.Entity<ArtifactType>(entity =>
        {
            entity.ToTable("artifact_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Phase).HasColumnName("phase").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).HasColumnName("code").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.IsMandatory).HasColumnName("is_mandatory");
            entity.Property(e => e.DefaultFormat).HasColumnName("default_format").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => new { e.Phase, e.Code }).IsUnique();

            entity.HasMany(e => e.Artifacts)
                .WithOne(a => a.ArtifactType)
                .HasForeignKey(a => a.ArtifactTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Artifacts
        modelBuilder.Entity<Artifact>(entity =>
        {
            entity.ToTable("artifacts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.PhaseId).HasColumnName("phase_id").IsRequired().HasMaxLength(50);
            entity.Property(e => e.ArtifactTypeId).HasColumnName("artifact_type_id");
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Author).HasColumnName("author").HasMaxLength(255);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.IsMandatory).HasColumnName("is_mandatory");
            entity.Property(e => e.ContentText).HasColumnName("content_text");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            // Configurar relaciones sin navegación inversa desde Phase
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Artifacts)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
