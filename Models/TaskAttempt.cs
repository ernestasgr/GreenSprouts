using System.ComponentModel.DataAnnotations;

namespace test.Models
{
	public class TaskAttempt
    {
        [Key]
        public int Task_Id { get; set; }
        public string? User { get; set; }
        public int Failures { get; set; }
    }
}