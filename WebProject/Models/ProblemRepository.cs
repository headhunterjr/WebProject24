
using Microsoft.EntityFrameworkCore;

namespace WebProject.Models
{
    public class ProblemRepository : IProblemRepository
    {
        private readonly ProblemDbContext _context;

        public ProblemRepository(ProblemDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> AddProblemAsync(Problem problem)
        {
            _context.Problems.Add(problem);
            return await _context.SaveChangesAsync();
        }

        public int[,] GenerateSquareMatrix(int size)
        {
            Random random = new Random();
            int[,] matrix = new int[size, size];
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    matrix[i, j] = random.Next(-5, 6);
                }
            }
            return matrix;
        }

        public async Task<IEnumerable<Problem>> GetAllProblemsAsync()
        {
            return await _context.Problems.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<Problem?> GetProblemById(int id)
        {
            return await _context.Problems.FirstOrDefaultAsync(p => p.Id == id);
        }

        public long MultiplyMatrices(CurrentProblem problem, CancellationToken cancellationToken)
        {
            int size = problem.MatrixSize;
            int[,] resultMatrix = new int[size, size];
            long result = 0;

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    for (int k = 0; k < size; ++k)
                    {
                        resultMatrix[i, j] += problem.MatrixA[i, k] * problem.MatrixB[k, j];
                    }
                }
            }

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    result += resultMatrix[i, j];
                }
            }
            return result;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}
