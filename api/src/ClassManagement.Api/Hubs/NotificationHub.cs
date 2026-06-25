using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ClassManagement.Api.Hubs;

/// <summary>
/// Realtime channel for in-app notifications (MVP-7.5). Clients connect authenticated (JWT cookie or
/// <c>?access_token=</c> for the WebSocket upgrade); the server pushes <c>notificationsChanged</c> to a
/// user via SignalR's default user identifier (the NameIdentifier claim). The hub has no client-callable
/// methods — it is push-only.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub { }
