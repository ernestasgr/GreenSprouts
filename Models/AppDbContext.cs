using Microsoft.EntityFrameworkCore;

namespace test.Models
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
		public DbSet<Account>? Accounts{ get; set; }
        public DbSet<ProgrammingTask>? Tasks { get; set; }

        public DbSet<ResultTask>? Results_VM { get; set; }
        public DbSet<Article>? Articles { get; set; }
        public DbSet<UserRole>? Roles { get; set; } 
		public DbSet<TaskSolution>? Task_Solutions { get; set; }
		public DbSet<TaskAttempt>? Task_Attempts { get; set; }
		public DbSet<TaskHint>? Task_Hints { get; set; }
		public DbSet<SolutionView>? Solution_Views { get; set; }
	}
}