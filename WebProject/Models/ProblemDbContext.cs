using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace WebProject.Models
{
    public class ProblemDbContext : IdentityDbContext
    {
        public ProblemDbContext(DbContextOptions options) : base(options)
        { 
        }
        public DbSet<Problem> Problems { get; set; }
    }
}
