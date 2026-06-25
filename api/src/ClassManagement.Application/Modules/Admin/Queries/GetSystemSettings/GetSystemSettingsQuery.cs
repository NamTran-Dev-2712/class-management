using ClassManagement.Application.Modules.Admin.DTOs;

// All system settings for the admin settings screen (A7-06). Small fixed set — no paging.
public record GetSystemSettingsQuery : IRequest<IReadOnlyList<SystemSettingDto>>;

public class GetSystemSettingsQueryHandler
    : IRequestHandler<GetSystemSettingsQuery, IReadOnlyList<SystemSettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSystemSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SystemSettingDto>> Handle(
        GetSystemSettingsQuery request,
        CancellationToken ct
    )
    {
        var settings = await _unitOfWork.SystemSettings.GetAllAsync(ct);
        return settings
            .Select(s => new SystemSettingDto(
                s.Key,
                s.Value,
                s.ValueType,
                s.IsPublic,
                s.Description,
                s.UpdatedAt
            ))
            .ToList();
    }
}
