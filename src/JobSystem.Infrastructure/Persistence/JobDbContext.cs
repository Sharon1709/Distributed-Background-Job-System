using JobSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSystem.Infrastructure.Persistence
{
    public class JobDbContext : DbContext
    {
        public DbSet<Job> Jobs => Set<Job>();

        public JobDbContext(DbContextOptions<JobDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.Property(j => j.Payload).IsRequired();
                entity.HasIndex(j => j.Status);
            });
        }
    }
}
