// Admin view of all media assets (MVP-9, A9-01/A9-02). Optional owner filter by public id.
public sealed record GetAdminMediaQuery : MediaFilterQuery
{
    public Guid? OwnerPublicId { get; init; }
}
