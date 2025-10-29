using Microsoft.EntityFrameworkCore;
using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)          
                .WithMany(c => c.Transactions)    
                .HasForeignKey(t => t.CategoryId) 
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)           
                .WithMany(a => a.Transactions)    
                .HasForeignKey(t => t.AccountId) 
                .OnDelete(DeleteBehavior.Restrict); 

 
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Date);
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Type);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Type);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Type);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            
        }
    }
}