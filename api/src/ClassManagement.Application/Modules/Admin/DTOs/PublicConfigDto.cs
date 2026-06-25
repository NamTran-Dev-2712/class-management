namespace ClassManagement.Application.Modules.Admin.DTOs;

/// <summary>Public (anonymous-readable) app configuration sourced from is_public system settings (MVP-7.5).</summary>
public sealed record PublicConfigDto(string AppName, bool MaintenanceMode);
