using Bus.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    public class UserService(IBusService busService)
    {
        public async Task CreateUser()
        {
            User user = new User();
            
            for (int i = 0; i <= 100; i++)
            {
                user = new User
                {
                    Id = i,
                    Name = $"BurakTest{i}",
                    Email = $"BurakTest{i}@BurakTest{i}.com",
                    Phone = $"55512312{i:00}"
                };

                await busService.PublishWithNoAck(new Bus.Shared.Events.UserCreatedEvent(
                    UserId: user.Id,
                    UserName: user.Name,
                    Email: user.Email,
                    Phone: user.Phone)
                );
            }
        }
    }
}
