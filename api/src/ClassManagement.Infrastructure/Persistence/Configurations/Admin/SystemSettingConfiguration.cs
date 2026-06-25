using ClassManagement.Domain.Modules.Admin.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Admin;

// system_settings — key-value config. value is jsonb (raw JSON text). updated_at via trigger; updated_by
// SET NULL. created_at (from BaseEntity) is an as-built addition over the schema doc (harmless).
public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable(
            "system_settings",
            t =>
                t.HasCheckConstraint(
                    "chk_system_settings_value_type",
                    "value_type IN ('string', 'integer', 'boolean', 'json')"
                )
        );

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.Key).HasDatabaseName("uq_system_settings_key").IsUnique();

        builder.Property(s => s.Value).IsRequired().HasColumnType("jsonb");
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.ValueType).IsRequired().HasMaxLength(20).HasDefaultValue("string");
        builder.Property(s => s.IsPublic).HasDefaultValue(false);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
