using Microsoft.AspNetCore.SignalR;

namespace WebProject.Hubs
{
    public class ProblemHub : Hub
    {
        public async Task SendProgressUpdate(string stage, int matrixSize, string taskId, long? result = null)
        {
            await Clients.All.SendAsync("ReceiveProgressUpdate", stage, matrixSize, taskId, result);
        }
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
