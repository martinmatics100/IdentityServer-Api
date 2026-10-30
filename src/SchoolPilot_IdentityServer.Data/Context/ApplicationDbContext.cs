

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolPilot_IdentityServer.Data.Entities;

namespace SchoolPilot_IdentityServer.Data.Context
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> option)
            : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(option)
    {
        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Account)
                .WithMany(a => a.Users)
                .HasForeignKey(u => u.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
