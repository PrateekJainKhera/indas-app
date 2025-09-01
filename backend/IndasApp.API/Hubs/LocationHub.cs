using Microsoft.AspNetCore.SignalR;

namespace IndasApp.API.Hubs
{
    // We inherit from the base Hub class provided by SignalR.
    public class LocationHub : Hub
    {
        // This method will be called by our backend (TrackingController)
        // to send a location update to all connected clients.
        public async Task SendLocationUpdate(int userId, string userFullName, double latitude, double longitude)
        {
            // 'Clients.All' sends the message to EVERYONE connected to this hub.
            // 'ReceiveLocationUpdate' is the name of the function that the FRONTEND will be listening for.
            // We are sending an object with the user's details and their new location.
            await Clients.All.SendAsync("ReceiveLocationUpdate", new { userId, userFullName, latitude, longitude });
        }

        // This method runs when a new client (e.g., an Admin's browser) connects to the hub.
        public override async Task OnConnectedAsync()
        {
            // We can add logic here later, for example, adding the user to a specific group
            // like "TeamLeads" so we don't send updates to everyone.
            // For now, we'll just let them connect.
            await base.OnConnectedAsync();
            Console.WriteLine($"A client connected: {Context.ConnectionId}");
        }

        // This method runs when a client disconnects.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"A client disconnected: {Context.ConnectionId}");
        }
    }
}