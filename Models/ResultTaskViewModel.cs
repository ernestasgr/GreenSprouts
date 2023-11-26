using System;
using System.ComponentModel.DataAnnotations;

namespace test.Models
{
	public class ResultTaskViewModel
    {
        public List<ResultTask> ResultTasks { get; set; }
        public string Title { get; set; }
        public int Id { get; set; }
        public string Filter { get; set; }
    }
}
