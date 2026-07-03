using System.Linq.Expressions;
using ClassManagement.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ClassManagement.Infrastructure.Persistence.DbContext;

public sealed class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        long,
        IdentityUserClaim<long>,
        ApplicationUserRole,
        IdentityUserLogin<long>,
        IdentityRoleClaim<long>,
        IdentityUserToken<long>
    >
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassMembership> ClassMemberships => Set<ClassMembership>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuestionTag> QuestionTags => Set<QuestionTag>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
    public DbSet<ExamTag> ExamTags => Set<ExamTag>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentSnapshot> AssignmentSnapshots => Set<AssignmentSnapshot>();
    public DbSet<SnapshotQuestion> SnapshotQuestions => Set<SnapshotQuestion>();
    public DbSet<SnapshotOption> SnapshotOptions => Set<SnapshotOption>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
    public DbSet<ManualGrade> ManualGrades => Set<ManualGrade>();

    // Media & rich content (MVP-9)
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<QuestionMedia> QuestionMedia => Set<QuestionMedia>();
    public DbSet<SnapshotMedia> SnapshotMedia => Set<SnapshotMedia>();

    // Premium & payment (MVP-8)
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // Admin & moderation (MVP-7)
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename Identity auxiliary tables to snake_case
        modelBuilder.Entity<IdentityUserClaim<long>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<long>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserToken<long>>().ToTable("user_tokens");
        modelBuilder.Entity<IdentityRoleClaim<long>>().ToTable("role_claims");

        // Register citext extension (applied via raw SQL in migration)
        modelBuilder.HasPostgresExtension("citext");

        // Apply all IEntityTypeConfiguration<T> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter: WHERE deleted_at IS NULL for all ISoftDeletable entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.DeletedAt));
            var nullValue = Expression.Constant(null, typeof(DateTime?));
            var condition = Expression.Equal(property, nullValue);
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
