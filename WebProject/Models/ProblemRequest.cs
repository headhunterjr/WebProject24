namespace WebProject.Models
{
    public class ProblemRequest
    {
        public int MatrixSize { get; set; }
        public required string ConnectionId { get; set; }
        public string TaskId { get; set; } = Guid.NewGuid().ToString();
    }
}
