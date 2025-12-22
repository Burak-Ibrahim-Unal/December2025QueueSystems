using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    internal interface IUserService
    {
        void Create(User user);
    }
}
