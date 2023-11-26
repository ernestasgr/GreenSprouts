using System.ComponentModel.DataAnnotations;

namespace test.Models
{
	public class TaskSolution
    {
        [Key]
        public int TaskId { get; set; }
        public string? Instructions { get; set; }
        public string? References_To_Helpful_Pages { get; set; }
    }
}