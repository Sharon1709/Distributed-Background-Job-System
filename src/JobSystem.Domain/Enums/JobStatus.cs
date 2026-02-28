using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSystem.Domain.Enums
{
    public enum JobStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4
    }
}
