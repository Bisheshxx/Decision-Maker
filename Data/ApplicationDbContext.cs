using DecisionMaker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DecisionMaker.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Decision> Decision { get; set; }
        public DbSet<DecisionItem> DecisionItem { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Decision>()
                   .HasOne(u => u.AppUser)
                   .WithMany(d => d.Decisions)
                   .HasForeignKey(u => u.UserId);

            builder.Entity<DecisionItem>()
                    .HasOne(di => di.Decision)
                    .WithMany(d => d.DecisionItems)
                    .HasForeignKey(di => di.DecisionId);
            builder.Entity<RefreshToken>()
                   .HasOne(r => r.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}