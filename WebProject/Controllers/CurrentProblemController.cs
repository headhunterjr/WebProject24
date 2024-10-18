using Microsoft.AspNetCore.Mvc;
using WebProject.Models;

namespace WebProject.Controllers
{
    public class CurrentProblemController : Controller
    {
        private readonly IProblemRepository _problemRepository;

        public CurrentProblemController(IProblemRepository problemRepository)
        {
            _problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult NewTask()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SolveProblem([FromForm] int matrixSize)
        {
            DateTime timeOfIssue = DateTime.UtcNow;
            int[,] matrixA = _problemRepository.GenerateSquareMatrix(matrixSize);
            int[,] matrixB = _problemRepository.GenerateSquareMatrix(matrixSize);
            CurrentProblem currentProblem = new CurrentProblem
            {
                MatrixSize = matrixSize,
                MatrixA = matrixA,
                MatrixB = matrixB
            };
            long multiplicationResult = _problemRepository.MultiplyMatrices(currentProblem);

            Problem problem = new Problem
            {
                MatrixSize = matrixSize,
                Result = multiplicationResult,
                TimeOfIssue = timeOfIssue
            };
            await _problemRepository.AddProblemAsync(problem);
            return Ok(multiplicationResult);
        }
    }
}
