using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WebProject.Hubs;
using WebProject.Models;

namespace WebProject.Controllers
{
    public class CurrentProblemController : Controller
    {
        private readonly IProblemRepository _problemRepository;
        private readonly IHubContext<ProblemHub> _problemHubContext;

        private const int MaxConcurrentTasks = 5;
        private static int _currentActiveTasks = 0;
        private static readonly object _lock = new object();
        private static readonly ConcurrentDictionary<(string userId, int matrixSize), CancellationTokenSource> _taskCancellations
            = new ConcurrentDictionary<(string, int), CancellationTokenSource>();

        public CurrentProblemController(IProblemRepository problemRepository, IHubContext<ProblemHub> problemHubContext)
        {
            _problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
            _problemHubContext = problemHubContext ?? throw new ArgumentNullException(nameof(problemHubContext));
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
        public async Task<IActionResult> SolveProblem([FromBody] ProblemRequest problemRequest)
        {
            lock (_lock)
            {
                if (_currentActiveTasks >= MaxConcurrentTasks)
                {
                    return BadRequest("Cannot add a new task right now. Maximum concurrent tasks limit reached.");
                }
                _currentActiveTasks++;
            }

            TimeSpan timeout = TimeSpan.FromSeconds(30);
            string userId = problemRequest.ConnectionId;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var timeoutTask = Task.Delay(timeout, cts.Token);

                var calculationTask = Task.Run(async () =>
                {
                    try
                    {
                        DateTime timeOfIssue = DateTime.UtcNow;

                        await _problemHubContext.Clients.Client(userId).SendAsync("ReceiveProgressUpdate", "Generating Matrix A", problemRequest.MatrixSize);
                        int[,] matrixA = _problemRepository.GenerateSquareMatrix(problemRequest.MatrixSize);

                        await _problemHubContext.Clients.Client(userId).SendAsync("ReceiveProgressUpdate", "Generating Matrix B", problemRequest.MatrixSize);
                        int[,] matrixB = _problemRepository.GenerateSquareMatrix(problemRequest.MatrixSize);

                        await _problemHubContext.Clients.Client(userId).SendAsync("ReceiveProgressUpdate", "Multiplying Matrices", problemRequest.MatrixSize);
                        CurrentProblem currentProblem = new CurrentProblem
                        {
                            MatrixSize = problemRequest.MatrixSize,
                            MatrixA = matrixA,
                            MatrixB = matrixB
                        };
                        long multiplicationResult = _problemRepository.MultiplyMatrices(currentProblem);

                        await _problemHubContext.Clients.Client(userId).SendAsync("ReceiveProgressUpdate", "Calculating final result", problemRequest.MatrixSize, multiplicationResult);

                        Problem problem = new Problem
                        {
                            MatrixSize = problemRequest.MatrixSize,
                            Result = multiplicationResult,
                            TimeOfIssue = timeOfIssue
                        };

                        await _problemRepository.AddProblemAsync(problem);

                        return multiplicationResult;
                    }
                    finally
                    {
                        lock (_lock)
                        {
                            _currentActiveTasks--;
                        }
                    }
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

        [HttpPost]
        public IActionResult CancelProblem([FromBody] ProblemRequest problemRequest)
        {
            string userId = problemRequest.ConnectionId;
            int matrixSize = problemRequest.MatrixSize;

            if (_taskCancellations.TryGetValue((userId, matrixSize), out var cts))
            {
                cts.Cancel();
                _taskCancellations.TryRemove((userId, matrixSize), out _);
                _problemHubContext.Clients.Client(userId).SendAsync("ReceiveProgressUpdate", "Cancelled", matrixSize);
                return Ok("Task has been cancelled.");
            }
            else
            {
                return NotFound("Task not found or already completed.");
            }
        }
    }
    public class ProblemRequest
    {
        public int MatrixSize { get; set; }
        public string ConnectionId { get; set; } // Include the connection ID in the request model
    }
}