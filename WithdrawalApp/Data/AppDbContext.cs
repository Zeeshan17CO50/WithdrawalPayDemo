using Microsoft.EntityFrameworkCore;
using WithdrawalApp.Models;

namespace WithdrawalApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> option) : base(option) 
        { 
        
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Added seeding 1 wallet balance record
            modelBuilder.Entity<Wallet>().HasData(new Wallet { Id = 1, Balance = 1000m });
            base.OnModelCreating(modelBuilder);
        }
    }
}
