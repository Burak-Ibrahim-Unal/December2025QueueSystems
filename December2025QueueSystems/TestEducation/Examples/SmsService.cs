using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestEducation.Examples.Observer;

namespace TestEducation.Examples
{
    internal class SmsService : ISmsService, IUserObserver
    {
        private readonly ISmsService _smsService;

        public SmsService()
        {

        }


        public void Send(string phoneNumber, string message)
        {
            _smsService.Send(phoneNumber, message);
        }

        public void ProcessOtherOperations()
        {
            Console.WriteLine("Sms sent");
        }
    }
}

