using System;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Models
{
    public class DBcontext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            builder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string DB_HOST = Environment.GetEnvironmentVariable("DB_HOST");
            string DB_NAME = Environment.GetEnvironmentVariable("DB_NAME");
            string DB_USER = Environment.GetEnvironmentVariable("DB_USER");
            string DB_PASSWORD = Environment.GetEnvironmentVariable("DB_PASSWORD");
            optionsBuilder.UseNpgsql($"Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}");
        }
    }
}