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
        public IActionResult NewProblem()
        {
            return View();
        }
        public async Task<ActionResult<IEnumerable<Problem>>> ProblemHistory()
        {
            var allProblems = await _problemRepository.GetAllProblemsAsync();
            ProblemTableViewModel problemTableViewModel = new ProblemTableViewModel { Problems = allProblems };
            return View(problemTableViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SolveProblem([FromBody] int matrixSize)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(30);

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var timeoutTask = Task.Delay(timeout, cts.Token);

                var calculationTask = Task.Run(async () =>
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
                    return multiplicationResult;
                }, cts.Token);

                var completedTask = await Task.WhenAny(calculationTask, timeoutTask);

                if (completedTask == calculationTask)
                {
                    cts.Cancel();
                    return Ok(await calculationTask);
                }
                else
                {
                    return StatusCode(408, "Request Timeout: The calculation took too long.");
                }
            }
        }

    }
}
