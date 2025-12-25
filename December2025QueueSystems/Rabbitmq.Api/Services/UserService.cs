using Rabbitmq.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    public class UserService(IBusService busService)
    {
        public async Task CreateUser()
        {
            await busService.Publish(new Bus.Shared.Events.UserCreatedEvent(
                UserId: 1,
                UserName: "BurakTest1",
                Email: "burak@burak.com",
                Phone: "5551231212"));
        }
    }
}
