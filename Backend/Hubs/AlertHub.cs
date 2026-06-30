using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    public class AlertHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", 
                $"Connecté au hub d'alertes. Id: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}