namespace WebProject.Models
{
    public interface IProblemRepository
    {
        public Task<IEnumerable<Problem>> GetAllProblemsAsync();
        public Task<Problem?> GetProblemById(int id);
        public int[,] GenerateSquareMatrix(int size);
        public long MultiplyMatrices(CurrentProblem problem);
        public Task<int> AddProblemAsync(Problem problem);
        public Task<bool> SaveChangesAsync();
    }
}
