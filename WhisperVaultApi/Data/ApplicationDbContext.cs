using Microsoft.EntityFrameworkCore;
using WhisperVaultApi.Models; // Adjust namespace based on your Models location

namespace WhisperVaultApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Confession> Confessions { get; set; }
    }
}
