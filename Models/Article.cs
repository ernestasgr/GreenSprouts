using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
namespace test.Models
{
	public class Article
	{
		[Key]
		public int? Id { get; set; }

		[Required]
		public string? Title { get; set; }

		[Required]
		public string? Description { get; set; }

        [Required]
		public string? ImageName { get; set; }

		[Required]
		public DateTime? CreationDate { get; set; }
		[Required]
		public string Topic { get; set; } 
		[Required]
		public string ShortDescription { get; set; }
    }
}