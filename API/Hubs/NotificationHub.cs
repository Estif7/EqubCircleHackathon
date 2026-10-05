using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EkubCircle.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}
