using Microsoft.EntityFrameworkCore;

namespace Basic.Models
{
	public class AppDbContext : DbContext
	{
		public AppDbContext()
		{
		}

		public AppDbContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<Blog> Blogs { get; set; }
	}
}
