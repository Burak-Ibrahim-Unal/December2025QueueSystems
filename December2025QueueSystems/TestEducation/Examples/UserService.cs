using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal class UserService(IUserRepository userRepository)
    {
        public void Register(User user)
        {
            userRepository.Create(user);
        }
    }
}
