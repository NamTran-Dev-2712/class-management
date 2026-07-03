namespace ClassManagement.Infrastructure.Persistence.Configurations.Media;

/// <summary>
/// Maps the read-only <see cref="MediaView"/> projection to the <c>vw_media</c> view. Created via raw SQL
/// in the media migration; EF only needs its shape. No table is generated (<c>ToView</c>); the view
/// filters out soft-deleted rows and deliberately omits <c>storage_key</c> (BR-9-01).
/// </summary>
public sealed class MediaViewConfiguration : IEntityTypeConfiguration<MediaView>
{
    public void Configure(EntityTypeBuilder<MediaView> builder)
    {
        builder.ToView("vw_media");
        builder.HasKey(m => m.Id);
    }
}
