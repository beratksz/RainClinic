using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using rainclinic.Models;

namespace rainclinic.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties go here
        // public DbSet<YourEntity> YourEntities { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Appointment> Appointments { get; set; }


    }
}
