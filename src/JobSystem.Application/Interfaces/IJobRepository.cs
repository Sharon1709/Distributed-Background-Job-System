using JobSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSystem.Application.Interfaces
{
    public interface IJobRepository
    {
        Task AddAsync(Job job, CancellationToken cancellationToken);
        Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateAsync(Job job, CancellationToken cancellationToken);
    }
}
