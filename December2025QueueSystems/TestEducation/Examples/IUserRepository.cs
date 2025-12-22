using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    public class User
    {
    }

    internal interface IUserRepository
    {
        void Create(User user);
    }
}
