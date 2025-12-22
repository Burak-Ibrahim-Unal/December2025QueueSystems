using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    public class User
    {
        public string Email { get; set; }
    }

    internal interface IUserRepository
    {
        void Create(User user);
    }
}
