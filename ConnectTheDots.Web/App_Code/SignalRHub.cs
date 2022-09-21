using Microsoft.AspNet.SignalR;

namespace ConnectTheDots.Web
{
    public class SignalRHub : Hub
    {
        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(name, message);
        }
    }
}