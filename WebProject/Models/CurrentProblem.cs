namespace WebProject.Models
{
    public class CurrentProblem
    {
        public int MatrixSize { get; set; }
        public required int[,] MatrixA { get; set; }
        public required int[,] MatrixB { get; set; }
    }
}
