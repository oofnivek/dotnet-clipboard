using Microsoft.EntityFrameworkCore;
using DotNetClipboard.Models;

namespace DotNetClipboard.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Clipboard> Clipboards { get; set; }
    }
}
