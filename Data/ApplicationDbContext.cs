using Microsoft.EntityFrameworkCore;
using eCFR.Analyst.Models;

namespace eCFR.Analyst.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<AgencyTitle> Titles { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Ensure we use EcfrDb database
            modelBuilder.HasDefaultSchema("dbo");
        }
    }
}