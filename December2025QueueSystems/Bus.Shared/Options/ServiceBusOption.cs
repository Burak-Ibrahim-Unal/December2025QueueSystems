using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Shared.Options
{
    public class ServiceBusOption
    {
        public required string RabbitMqConnectionString { get; set; }
    }
}
