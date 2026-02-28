using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSystem.Application.Interfaces
{
    public interface IJobQueue
    {
        Task PublishAsync(Guid jobId, CancellationToken cancellationToken);
    }
}
