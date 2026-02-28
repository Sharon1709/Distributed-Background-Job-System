using JobSystem.Application.Interfaces;
using JobSystem.Domain.Entities;
using JobSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSystem.Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly JobDbContext _context;

        public JobRepository(JobDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Job job, CancellationToken cancellationToken)
        {
            await _context.Jobs.AddAsync(job, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Jobs
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task UpdateAsync(Job job, CancellationToken cancellationToken)
        {
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
