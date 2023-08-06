using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.Server.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string me, string touser, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", me, touser, message);
    }
}