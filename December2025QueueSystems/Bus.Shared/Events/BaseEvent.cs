using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Shared.Events
{
    public abstract record BaseEvent
    {
        public Guid MessageId => Guid.NewGuid();
        public DateTime Created => DateTime.UtcNow;
    }
}
