using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace virgollanding.Models
{
    public class AppDbContext : IdentityDbContext<IdentityUser<int> , IdentityRole<int> , int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<SiteSettings> SiteSettings {get; set;}
        public DbSet<VerificationCodeModel> VerificationCodes {get; set;}
        public DbSet<ReqForm> ReqForms {get; set;}

    }
}