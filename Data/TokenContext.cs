using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TokenTest.Models;

namespace Token.Data
{
    public class TokenContext : DbContext
    {
        public TokenContext(DbContextOptions<TokenContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Person> Person { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<Note> Note { get; set; }
        public DbSet<Goal> Goal { get; set; }
        public DbSet<DataEntry> Data { get; set; }
        public DbSet<UserToTokenLookup> UserToTokenLookup { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Person>().ToTable("Person");
            modelBuilder.Entity<Student>().ToTable("Student");
            modelBuilder.Entity<Note>().ToTable("Note");
            modelBuilder.Entity<Goal>().ToTable("Goal");
            modelBuilder.Entity<DataEntry>().ToTable("Data");
            modelBuilder.Entity<UserToTokenLookup>().ToTable("UserToTokenLookup");
        }

    }
}
