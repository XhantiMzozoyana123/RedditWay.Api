using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Domain
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        // Design-time constructor for migrations
        public ApplicationDbContext() : base(DesignTimeDbContextOptions())
        {
        }

        private static DbContextOptions<ApplicationDbContext> DesignTimeDbContextOptions()
        {
            // Build the path to the PetGroomer.Api project
            var webProjectPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "RedditWay.Api");

            // Load the configuration from appsettings.json in the PetGroomer.Api project
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(webProjectPath) // Set the base path to the PetGroomer.Api directory
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load appsettings.json
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Choose the appropriate database provider here
            builder.UseSqlServer(connectionString); // Or UseSqlite, UseNpgsql, etc.

            return builder.Options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and table names if needed
        }

        // DbSets for your entities

        public DbSet<RedditUsers> RedditUsers { get; set; }

        public DbSet<RedditAccounts> RedditAccounts { get; set; }

        public DbSet<RedditInbox> RedditInboxes { get; set; }
        
        public DbSet<RedditTemplates> RedditTemplates { get; set; }

        public DbSet<RedditYouTubeChannels> RedditYouTubeChannels { get; set; }

        public DbSet<AppInfo> AppInfo { get; set; }
    }
}
