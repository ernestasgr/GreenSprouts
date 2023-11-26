using System;
using System.ComponentModel.DataAnnotations;

namespace test.Models
{
	public class UserRole
	{
		[Key]
		public int Id { get; set; }

        [Required]
		public string? Role { get; set; }
	}
}