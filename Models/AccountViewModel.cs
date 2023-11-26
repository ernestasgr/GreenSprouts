using System;
using System.ComponentModel.DataAnnotations;

namespace test.Models
{
	public class AccountViewModel
	{
		[Key]
		public string? Nickname { get; set; }

		[Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
		public string? Email { get; set; }

		[Required]
		public string? Hash { get; set; }

		public string? Salt { get; set; }

        [Required]
		public int? Role { get; set; }

		public string? RoleName { get; set; }
	}
}