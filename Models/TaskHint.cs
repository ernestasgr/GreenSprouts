using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace test.Models
{
    [PrimaryKey(nameof(Task_Id), nameof(Number))]
	public class TaskHint
    {
        public int Task_Id { get; set; }
        public string? Description { get; set; }
        public int Number { get; set; }
    }
}