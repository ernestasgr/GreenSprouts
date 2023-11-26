using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace test.Models
{
    [PrimaryKey(nameof(Task_Id), nameof(User))]
	public class SolutionView
    {
        public int Task_Id { get; set; }
        public string? User { get; set; }
    }
}