using JobSystem.Application.Interfaces;
using JobSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSystem.Application.Jobs
{
    public class CreateJobHandler
    {
        private readonly IJobRepository _repository;
        private readonly IJobQueue _queue;

        public CreateJobHandler(IJobRepository repository, IJobQueue queue)
        {
            _repository = repository;
            _queue = queue;
        }

        public async Task<Guid> HandleAsync(string payload, CancellationToken cancellationToken)
        {
            var job = new Job(payload);

            await _repository.AddAsync(job, cancellationToken);

            await _queue.PublishAsync(job.Id, cancellationToken);

            return job.Id;
        }
    }
}
