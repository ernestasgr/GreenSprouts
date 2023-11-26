namespace test.Models
{
	public class TaskSolutionViewModel
    {
        public string TaskName { get; set; }
        public Dictionary<string, string>? OptimalCodeByLanguages { get; set; }
        public List<string>? StepByStepInstructions { get; set; }
        public List<string>? ReferencesToHelpfulPages { get; set; }
        public List<string>? ReferenceShortName { get; set; }

        public TaskSolutionViewModel() 
        {
            OptimalCodeByLanguages = new Dictionary<string, string>();
            StepByStepInstructions = new List<string>();
            ReferencesToHelpfulPages = new List<string>();
            ReferenceShortName = new List<string>();
        }
    }
}