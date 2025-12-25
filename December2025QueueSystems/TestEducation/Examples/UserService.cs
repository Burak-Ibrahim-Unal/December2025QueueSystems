using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestEducation.Examples.Observer;

namespace TestEducation.Examples
{
    internal class UserService(IUserRepository userRepository,UserSubject userSubject)
    {
        public void Register(User user)
        {
            userRepository.Create(user);
            //emailService.Send(user.Email,"TestSubject","TestContent");
            //smsService.Send(user.Phone, "TestMessage");
            //discountService.Apply(user.Id, 20, "TestDicountCode");

            userSubject.Notify(user);
        }
    }
}
