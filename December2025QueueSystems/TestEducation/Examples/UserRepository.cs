using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal class UserRepository : IUserRepository
    {
        public void Create(User user)
        {
            Console.WriteLine($"'{user.Email}' User Created...");
        }
    }
}
