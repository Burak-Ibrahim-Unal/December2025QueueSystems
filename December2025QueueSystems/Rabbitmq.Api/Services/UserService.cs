using Bus.Shared;
using Bus.Shared.Events;
using Rabbitmq.Api.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    public class UserService(IBusService busService,AppDbContext appDbContext)
    {
        public async Task CreateUser()
        {
            User user = new User();
            
            for (int i = 0; i <= 100; i++)
            {
                user = new User
                {
                    Id = i,
                    UserName = $"BurakTest{i}",
                    Email = $"BurakTest{i}@BurakTest{i}.com",
                };

                await busService.PublishWithNoAck(new UserCreatedEvent(
                    UserId: user.Id,
                    UserName: user.UserName,
                    Email: user.Email
                ));
            }
        }
    }
}
