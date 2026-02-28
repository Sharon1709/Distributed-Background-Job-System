using JobSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSystem.Domain.Entities
{
    public class Job
    {
        public Guid Id { get; private set; }
        public string Payload { get; private set; }
        public JobStatus Status { get; private set; }
        public int RetryCount { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public string? ErrorMessage { get; private set; }

        public const int MaxRetries = 3;

        public bool CanRetry() => RetryCount < MaxRetries;

        private Job() { } // For EF

        public Job(string payload)
        {
            Id = Guid.NewGuid();
            Payload = payload;
            Status = JobStatus.Pending;
            RetryCount = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkProcessing()
        {
            Status = JobStatus.Processing;
        }

        public void MarkCompleted()
        {
            Status = JobStatus.Completed;
            ProcessedAt = DateTime.UtcNow;
        }

        public void MarkFailed(string error)
        {
            RetryCount++;
            Status = JobStatus.Failed;
            ErrorMessage = error;
        }
    }
}
