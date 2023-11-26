using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace test.Models
{
    public class ResultTask
    {
        [Key]
        public int Idresults_Vm { get; set; }

        public decimal Avg_Cpu_Consumption { get; set; }

        public decimal Avg_Ram_Consumption { get; set; }

        public decimal Avg_Energy_Consumption { get; set; }

        public decimal Avg_Time_Compiling { get; set; }

        public int Test_Accepted_Amount { get; set; }

        public int Idselected_Task { get; set; }
        public string? Name { get; set; }
    }
}