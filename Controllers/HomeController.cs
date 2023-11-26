using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using test.Models;
using test.Services;

namespace test.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext db;

    public HomeController(ILogger<HomeController> logger, AppDbContext db)
    {
        _logger = logger;
        this.db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Tasks()
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

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AboutUs()
    {
        return View();
    }
    public IActionResult Compiler()
    {
        return View();
    }

    public IActionResult Admin()
    {
        return View();
    }

    private double CalculateEnergy(double load, double hours)
    {
        var TDP = 110;
        return 2 * TDP * load / 100 * hours;
    }

    private double CalculateCO2(double wh)
    {
        const double gramsPerWhInNorthEurope = 0.398;
        return wh * gramsPerWhInNorthEurope; 
    }
    RabbitSenderService sender = new RabbitSenderService();
    RabbitReceiverService receiver = new RabbitReceiverService();
    [HttpPost]
    public async Task<IActionResult> ShowCode([FromBody] string text)
    {
        var username = "pvpvmadmin";
        var ip = "20.166.77.219";
        var password = "do8MmUot8hVL9dn";
        var output = "";
        var data = text.Split("veryultraspecialsplitterpleasedontuseasvariablename");
        string language = data[2];

        var code = data[1];
        var id = data[0];

        if(language == "python")
        {
            sender.SendPyData(code);
            return await SendPythonToVM(id, username, password, ip);
        }
        else if(language == "cpp")
        {
            sender.SendCppData(code);
            return await SendCppToVM(id, username, password, ip);
        }
        return Json(output);
    }
    private async Task<JsonResult> SendPythonToVM(string id, string username, string password, string ip)
    {
        string code = receiver.ReceivePyData();
        string output = "";
        try
        {
            using (var client = new SshClient(ip, username, password))
            {
                client.Connect();
                code = code.Replace("\"", "\\\"");
                client.CreateCommand($"echo \"{code}\" > pycode.py").Execute();
                client.CreateCommand("python3 pycode.py > code 2>&1").Execute();
                client.CreateCommand($"/usr/bin/time -f \"\\nTime elapsed: %E,Memory used: %M KB,CPU usage: %P\" ./code_test.sh {id} > output.txt 2>&1").Execute();

                var codeOutput = client.CreateCommand("cat code").Execute();
                output += codeOutput;
                var codeErrors = client.CreateCommand("cat code_errors.txt").Execute();
                var tests = client.CreateCommand("cat passed_tests.txt").Execute();
                var testData = tests.Split('/');
                var passedTests = testData[0];
                var totalTests = testData[1].TrimEnd('\n');

                output += $"Passed tests: {tests}\n";

                var resultFile = client.CreateCommand("cat output.txt").Execute();
                var lines = resultFile.Split("\n");
                string resultItemsLine = lines[lines.Length - 2];
                string[] resultItems = resultItemsLine.Split(",");

                int testValue = int.Parse(passedTests);

                var results = await getResults(resultItems, int.Parse(id), testValue, passedTests == totalTests);
                output += results;

                return Json(output);
            }
        }
        catch (SocketException)
        {
            output = "Connection problem to VM. Try again!splitter0";
            return Json(output);
        }
    }
    private async Task<JsonResult> SendCppToVM(string id, string username, string password, string ip)
    {
        string code = receiver.ReceiveCppData();
        string output = "";
        try
        {
            using (var client = new SshClient(ip, username, password))
            {
                client.Connect();
                code = code.Replace("\"", "\\\"");
                client.CreateCommand($"echo \"{code}\" > code.cpp").Execute();
                var compileError = client.CreateCommand("g++ code.cpp -o code 2>&1").Execute();

                if (string.IsNullOrEmpty(compileError))
                {
                    client.CreateCommand($"/usr/bin/time -f \"\\nTime elapsed: %E,Memory used: %M KB,CPU usage: %P\" ./code_test.sh {id} > output.txt 2>&1").Execute();

                    var codeOutput = client.CreateCommand("cat code_output.txt").Execute();
                    output += codeOutput;
                    var codeErrors = client.CreateCommand("cat code_errors.txt").Execute();
                    var tests = client.CreateCommand("cat passed_tests.txt").Execute();
                    var testData = tests.Split('/');
                    var passedTests = testData[0];
                    var totalTests = testData[1].TrimEnd('\n');

                    output += $"Passed tests: {tests}\n";
                    if (!string.IsNullOrEmpty(codeErrors))
                    {
                        return Json(codeErrors);
                    }
                    int testValue = int.Parse(passedTests);
                    var resultFile = client.CreateCommand("cat output.txt").Execute();
                    var lines = resultFile.Split("\n");
                    string resultItemsLine = lines[lines.Length - 2];
                    Console.WriteLine(resultItemsLine);
                    string[] resultItems = resultItemsLine.Split(",");
                    var results = await getResults(resultItems, int.Parse(id), testValue, passedTests == totalTests);
                    var outputsFile = client.CreateCommand("cat outputs.txt").Execute();
                    outputsFile = outputsFile.Replace("\n", "<br>");
                    output += results + "splitter" + outputsFile;
                }
                else
                {
                    output = compileError;
                }
                client.Disconnect();
            }
            
        }
        catch (SocketException)
        {
            output = "Connection problem to VM. Try again!splitter0";
            return Json(output);
        }
        return Json(output);
    }
    public async Task<string> getResults(string[] resultItems, int id, int passedTests, bool passed)
    {
        string output = "";
        List<string> resultValues = new List<string>();

        foreach (var item in resultItems)
        {
            var value = item.Split(": ");
            resultValues.Add(value[1]);
        }

        string timeElapsed = resultValues[0];
        string memoryUsed = resultValues[1];
        string cpuPercent = resultValues[2];

        string[] timeParts = timeElapsed.Split(':');
        int minutes = int.Parse(timeParts[0]);
        double seconds = double.Parse(timeParts[1], System.Globalization.CultureInfo.InvariantCulture);
        double milliseconds = (minutes * 60 + seconds) * 1000;

        double AvgCpuConsumption = Convert.ToDouble(cpuPercent.Replace("%", ""));
        double AvgRamConsumption = Convert.ToDouble(memoryUsed.Replace("KB", ""));
        double AvgTimeCompiling = milliseconds;
        int consumption = 1;
        string hints = "";
        Console.WriteLine("Cpu: ", AvgCpuConsumption);
        if (AvgCpuConsumption > 0)
        {
            output += $"Average Memory Usage {AvgRamConsumption} KB\n";
            output += $"Average CPU Usage {AvgCpuConsumption}%\n";
            output += $"Runtime: {AvgTimeCompiling} ms\n";

            var usedEnergy = CalculateEnergy(AvgCpuConsumption, AvgTimeCompiling / 1000 / 60 / 60);
            var usedCO2 = CalculateCO2(usedEnergy);
            output += $"Used energy (wh) {usedEnergy:F3}\n";
            output += $"Used CO2 (g) {usedCO2:F3}\n";
            var newTask = new ResultTask
            {
                Avg_Cpu_Consumption = Convert.ToDecimal(AvgCpuConsumption),
                Avg_Ram_Consumption = Convert.ToDecimal(AvgRamConsumption),
                Avg_Time_Compiling = Convert.ToDecimal(AvgTimeCompiling),
                Avg_Energy_Consumption = Convert.ToDecimal(usedEnergy),
                Test_Accepted_Amount = passedTests,
                Idselected_Task = id,
                Name = @User.Identity.Name
            };
            consumption = ShowConsumptionValue(id, usedEnergy, AvgTimeCompiling);

            var solutionViewed = db.Solution_Views.Where(sv => sv.Task_Id == id && sv.User == @User.Identity.Name);
            if(passed && !solutionViewed.Any())
            {
                var existingResult = db.Results_VM.Where(rvm => rvm.Name == @User.Identity.Name && rvm.Idselected_Task == id).FirstOrDefault();
                if(existingResult == null)
                {
                    db.Results_VM.Update(newTask);
                }
                else
                {
                    if(newTask.Avg_Energy_Consumption < existingResult.Avg_Energy_Consumption ||
                    (Math.Round(newTask.Avg_Energy_Consumption, 3) - existingResult.Avg_Energy_Consumption <= 0) && 
                    newTask.Avg_Time_Compiling < existingResult.Avg_Time_Compiling)
                    {
                        existingResult.Avg_Cpu_Consumption = newTask.Avg_Cpu_Consumption;
                        existingResult.Avg_Ram_Consumption = newTask.Avg_Ram_Consumption;
                        existingResult.Avg_Time_Compiling = newTask.Avg_Time_Compiling;
                        existingResult.Avg_Energy_Consumption = newTask.Avg_Energy_Consumption;
                        existingResult.Test_Accepted_Amount = newTask.Test_Accepted_Amount;
                    }
                }
                await db.SaveChangesAsync();
            }
            else
            {
                var attempt = db.Task_Attempts.Where(ta => ta.Task_Id == id && ta.User == @User.Identity.Name);
                var taskHints = db.Task_Hints.Where(th => th.Task_Id == id);
                if(attempt.Any())
                {
                    var userAttempt = attempt.First();
                    int failures = userAttempt.Failures + 1;
                    userAttempt.Failures++;
                    for(int i = 0; i < Math.Min(taskHints.Count(), failures); i++)
                    {
                        hints += taskHints.Where(th => th.Number == i+1).First().Description + "hint";
                    }
                }
                else
                {
                    hints = taskHints.First().Description;
                    var newAttempt = new TaskAttempt
                    {
                        Task_Id = id,
                        User = @User.Identity.Name,
                        Failures = 1
                    };
                    db.Task_Attempts.Add(newAttempt);
                }
                await db.SaveChangesAsync();
            }
        }
        return output + "splitter" + consumption.ToString() + "splitter" + hints;
    }
    [HttpPost]
    public int ShowConsumptionValue(int id, double energy, double time)
    {
        int testConsumptionValue = 0;
        var task = db.Tasks.Where(t => t.IdTask == id).First();
        if(energy >= 2 * Convert.ToDouble(task.Expected_Avg_Energy_Consumption))
        {
            testConsumptionValue = 180;
        }
        else if(energy > Convert.ToDouble(task.Expected_Avg_Energy_Consumption))
        {
            testConsumptionValue = Convert.ToInt32(Math.Round((energy - Convert.ToDouble(task.Expected_Avg_Energy_Consumption)) * 180 / Convert.ToDouble(task.Expected_Avg_Energy_Consumption))); 
        }
        return testConsumptionValue;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
