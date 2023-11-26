using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test.Models
{
	public class ProgrammingTask
	{
		[Key]
		public int IdTask { get; set; }

		[Required]
		public string Title { get; set; }

		[Required]
		public string Description { get; set; }

		[Required]
		public string Difficulty { get; set; }

		[Required]
		public string Type { get; set; }

		[Required]
		public decimal Expected_Avg_Cpu_Consumption { get; set; }

		[Required]
		public decimal Expected_Avg_Ram_Consumption { get; set; }

		[Required]
		public decimal Expected_Avg_Energy_Consumption { get; set; }

		[Required]
		public decimal Expected_Avg_Time_Compiling { get; set; }
		[NotMapped]
		public List<TaskHint> Hints { get; set; }
	}
}