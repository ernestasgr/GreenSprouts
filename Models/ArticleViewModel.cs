using System;
using System.ComponentModel.DataAnnotations;

namespace test.Models
{
	public class ArticleViewModel
    {
        public List<Article> Articles { get; set; }
        public List<string> UniqueTopics { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
    }
}
