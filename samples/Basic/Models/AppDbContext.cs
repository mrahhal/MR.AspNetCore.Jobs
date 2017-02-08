using Microsoft.EntityFrameworkCore;

namespace Basic.Models
{
	public class AppDbContext : DbContext
	{
		public AppDbContext()
		{
		}

		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<Blog> Blogs { get; set; }
	}
}
