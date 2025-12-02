using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public DbSet<ArtifactVersion> ArtifactVersions => Set<ArtifactVersion>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<ProjectUserRole> ProjectUserRoles => Set<ProjectUserRole>();
    public DbSet<UserStory> UserStories => Set<UserStory>();
    public DbSet<IterationScope> IterationScopes => Set<IterationScope>();
    public DbSet<ProjectInvitation> ProjectInvitations => Set<ProjectInvitation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    //HU-008
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<IterationLog> IterationLogs => Set<IterationLog>();


    public override int SaveChanges()
    {
        ConvertDateTimesToUtc();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDateTimesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ConvertDateTimesToUtc()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Setear CreatedAt y UpdatedAt automáticamente
            if (entry.State == EntityState.Added)
            {
                var createdAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAtProp != null && createdAtProp.CurrentValue == null || 
                    (createdAtProp?.CurrentValue is DateTime dt && dt == DateTime.MinValue))
                {
                    createdAtProp.CurrentValue = DateTime.UtcNow;
                }

                var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedAtProp != null && updatedAtProp.CurrentValue == null || 
                    (updatedAtProp?.CurrentValue is DateTime dt2 && dt2 == DateTime.MinValue))
                {
                    updatedAtProp.CurrentValue = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedAtProp != null)
                {
                    updatedAtProp.CurrentValue = DateTime.UtcNow;
                }
            }

            // Convertir todos los DateTime a UTC
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime))
                {
                    var value = (DateTime?)property.CurrentValue;
                    if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                    }
                    else if (value.HasValue && value.Value.Kind == DateTimeKind.Local)
                    {
                        property.CurrentValue = value.Value.ToUniversalTime();
                    }
                }
                else if (property.Metadata.ClrType == typeof(DateTime?))
                {
                    var value = (DateTime?)property.CurrentValue;
                    if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                    }
                    else if (value.HasValue && value.Value.Kind == DateTimeKind.Local)
                    {
                        property.CurrentValue = value.Value.ToUniversalTime();
                    }
                }
            }
        }
    }

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
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");

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
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
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

        // Roles
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");

            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ArtifactVersions
        modelBuilder.Entity<ArtifactVersion>(entity =>
{
    entity.ToTable("artifact_versions");
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Id).HasColumnName("id");
    entity.Property(e => e.ArtifactId).HasColumnName("artifact_id");
    entity.Property(e => e.VersionNumber).HasColumnName("version_number");
    entity.Property(e => e.Date).HasColumnName("date").HasColumnType("timestamp");
    entity.Property(e => e.AuthorId).HasColumnName("author_id");
    entity.Property(e => e.ChangeLog).HasColumnName("change_log");
    entity.Property(e => e.FileUrl).HasColumnName("file_url");

    entity.HasIndex(e => new { e.ArtifactId, e.VersionNumber }).IsUnique();

    entity.HasOne(e => e.Artifact)
        .WithMany(a => a.Versions)
        .HasForeignKey(e => e.ArtifactId)
        .OnDelete(DeleteBehavior.Cascade);
});


        // ProjectUserRoles
        modelBuilder.Entity<ProjectUserRole>(entity =>
        {
            entity.ToTable("project_user_roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.InvitedBy).HasColumnName("invited_by");
            entity.Property(e => e.InvitedAt).HasColumnName("invited_at").HasColumnType("timestamp");
            entity.Property(e => e.AcceptedAt).HasColumnName("accepted_at").HasColumnType("timestamp");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");

            entity.HasIndex(e => new { e.ProjectId, e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Inviter)
                .WithMany()
                .HasForeignKey(e => e.InvitedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // UserStories
        modelBuilder.Entity<UserStory>(entity =>
        {
            entity.ToTable("user_stories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.AcceptanceCriteria).HasColumnName("acceptance_criteria");
            entity.Property(e => e.Priority).HasColumnName("priority").HasMaxLength(20);
            entity.Property(e => e.StoryPoints).HasColumnName("story_points");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // IterationScope
        modelBuilder.Entity<IterationScope>(entity =>
        {
            entity.ToTable("iteration_scope");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.IterationId).HasColumnName("iteration_id");
            entity.Property(e => e.ItemType).HasColumnName("item_type").IsRequired().HasMaxLength(20);
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EstimatedHours).HasColumnName("estimated_hours").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");

            entity.HasIndex(e => new { e.IterationId, e.ItemType, e.ItemId }).IsUnique();

            entity.HasOne(e => e.Iteration)
                .WithMany()
                .HasForeignKey(e => e.IterationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AssignedUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ProjectInvitations
        modelBuilder.Entity<ProjectInvitation>(entity =>
        {
            entity.ToTable("project_invitations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.InvitedEmail).HasColumnName("invited_email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.InvitedBy).HasColumnName("invited_by");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.InvitationToken).HasColumnName("invitation_token").IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp");
            entity.Property(e => e.AcceptedAt).HasColumnName("accepted_at").HasColumnType("timestamp");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");

            entity.HasIndex(e => e.InvitationToken).IsUnique();

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Inviter)
                .WithMany()
                .HasForeignKey(e => e.InvitedBy)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Notifications
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type).HasColumnName("type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).HasColumnName("message").IsRequired();
            entity.Property(e => e.RelatedEntityType).HasColumnName("related_entity_type").HasMaxLength(50);
            entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.ActionUrl).HasColumnName("action_url").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.ReadAt).HasColumnName("read_at").HasColumnType("timestamp");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        //HU-008
        modelBuilder.Entity<TestCase>(entity =>
    {
            entity.ToTable("test_cases");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArtifactId).HasColumnName("artifact_id");
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.Expected).HasColumnName("expected").IsRequired();
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.EvidenceUrl).HasColumnName("evidence_url");

            entity.HasOne(e => e.Artifact)
                .WithMany(a => a.TestCases)
                .HasForeignKey(e => e.ArtifactId)
                .OnDelete(DeleteBehavior.Cascade);
    });
        modelBuilder.Entity<IterationLog>(entity =>
    {
            entity.ToTable("iteration_logs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArtifactId).HasColumnName("artifact_id");
            entity.Property(e => e.Date).HasColumnName("date").HasColumnType("timestamp");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Progress).HasColumnName("progress");

            entity.HasOne(e => e.Artifact)
                .WithMany(a => a.IterationLogs)
                .HasForeignKey(e => e.ArtifactId)
                .OnDelete(DeleteBehavior.Cascade);
    });



    }
}
