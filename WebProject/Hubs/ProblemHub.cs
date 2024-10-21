using Microsoft.AspNetCore.SignalR;

namespace WebProject.Hubs
{
    public class ProblemHub : Hub
    {
        public async Task SendProgressUpdate(string userId, string stage, int matrixSize, long? result = null)
        {
            await Clients.User(userId).SendAsync("ReceiveProgressUpdate", stage, matrixSize, result);
        }
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
