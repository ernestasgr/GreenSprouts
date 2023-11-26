using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test.Models;
using System.IO;
using Renci.SshNet;
using Microsoft.AspNetCore.Authorization;

namespace test.Controllers
{
    [Authorize(Roles = "User, Administrator")]
    public class TaskController : Controller
    {
        private readonly AppDbContext db;

        public TaskController(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var tasks = await db.Tasks.ToListAsync();
            if (tasks == null)
            {
                return View("Error");
            }
            else
            {
                return View(tasks);
            }
        }

        public async Task<IActionResult> IndexOne(int id)
        {

            var exercise =await db.Tasks.FirstOrDefaultAsync(x => x.IdTask == id);

            if(exercise == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ProgrammingTask
            {
                IdTask = exercise.IdTask,
                Title = exercise.Title,
                Description = exercise.Description,
                Type = exercise.Type,
                Difficulty = exercise.Difficulty
            };

            model.Hints = db.Task_Hints.Where(th => th.Task_Id == model.IdTask).ToList();
            var attempt = db.Task_Attempts.Where(ta => ta.User == @User.Identity.Name && ta.Task_Id == model.IdTask);
            int failures = 0;
            if(attempt.Any())
            {
                failures = attempt.First().Failures;
                model.Hints = model.Hints.Take(failures).ToList();
            }
            else
            {
                model.Hints = new List<TaskHint>();
            }

            return View(model);
        }

        public async Task<IActionResult> Leaderboard(int id, string filter = "energy") 
        {
            var taskResults = await db.Results_VM.Where(x => x.Idselected_Task == id).ToListAsync();

            if (filter == "time")
            {
                taskResults = taskResults.OrderBy(r => r.Avg_Time_Compiling).Take(100).ToList();
            }
            else 
            {
                taskResults = taskResults.OrderBy(r => r.Avg_Energy_Consumption).Take(100).ToList();
            }

            var taskName = db.Tasks
                   .Where(t => t.IdTask == id)
                   .Select(t => t.Title)
                   .FirstOrDefault();

            var model = new ResultTaskViewModel 
            {
                ResultTasks = taskResults,
                Title = taskName,
                Id = id,
                Filter = filter
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Solution(int id)
        {
            var username = "pvpvmadmin";
            var ip = "20.166.77.219";
            var password = "do8MmUot8hVL9dn";

            var taskName = await db.Tasks
                        .Where(x => x.IdTask == id)
                        .Select(x => x.Title)
                        .SingleOrDefaultAsync();
            if(taskName == null) 
            {
                return View(null);
            }

            var taskSolution = await db.Task_Solutions
                            .Where(x => x.TaskId == id)
                            .SingleOrDefaultAsync();
            if(taskSolution == null)
            {
                return View(null);
            }

            var solutionViewed = db.Solution_Views.Where(sv => sv.Task_Id == id && sv.User == @User.Identity.Name);
            if(!solutionViewed.Any())
            {
                var solutionView = new SolutionView {Task_Id = id, User = @User.Identity.Name};
                db.Solution_Views.Add(solutionView);
                await db.SaveChangesAsync();
            }

            using(var client = new SshClient(ip, username, password))
            {
                client.Connect();
                var taskFiles = client.CreateCommand($"ls task{id}/code").Execute();
                var fileNames = taskFiles.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                var optimalCodes = new Dictionary<string, string>();

                foreach (var fileName in fileNames)
                {
                    var optimalCode = client.CreateCommand($"cat task{id}/code/{fileName}").Execute();
                    string fileExtension = Path.GetExtension(fileName);

                    if(fileExtension == ".py")
                    {
                        optimalCodes.Add(key:"Python", value:optimalCode);
                    }

                    if(fileExtension == ".cpp")
                    {
                        optimalCodes.Add(key:"C++", value:optimalCode);
                    }
                }

                var instructions = taskSolution.Instructions.Split("\\n").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                if(taskSolution.References_To_Helpful_Pages == null) 
                {
                    var taskSolutionWithoutLinks = new TaskSolutionViewModel
                    {
                       TaskName = taskName,
                        OptimalCodeByLanguages = optimalCodes,
                        StepByStepInstructions = instructions,
                        ReferencesToHelpfulPages = null,
                        ReferenceShortName = null 
                    };

                    return View(taskSolutionWithoutLinks);
                }
                var linksAndNames = taskSolution.References_To_Helpful_Pages.Split("\\n")
                    .Select(s => s.Split("@"))
                    .Where(parts => parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                    .Select(parts => new { Link = parts[0].Trim(), ShortName = parts[1].Trim() })
                    .ToList();

                var linksToPages = linksAndNames.Select(l => l.Link).ToList();
                var linkShortNames = linksAndNames.Select(l => l.ShortName).ToList();

                var taskSolutionViewModel = new TaskSolutionViewModel
                {
                    TaskName = taskName,
                    OptimalCodeByLanguages = optimalCodes,
                    StepByStepInstructions = instructions,
                    ReferencesToHelpfulPages = linksToPages,
                    ReferenceShortName = linkShortNames
                };

                return View(taskSolutionViewModel);
            }
        }
    }
}