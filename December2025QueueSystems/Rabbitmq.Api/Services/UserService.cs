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
        public void Register(User user)
        {

        }
    }
}
