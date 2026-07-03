using ClassManagement.Application.Modules.Media.DTOs;

// Teacher's current media storage usage vs. their plan cap (MVP-9, T9-05).
public sealed record GetStorageUsageQuery : IRequest<StorageUsageDto>;
