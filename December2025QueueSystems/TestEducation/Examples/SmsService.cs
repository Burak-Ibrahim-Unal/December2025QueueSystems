using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal class SmsService(ISmsService smsService)
    {
        public void SendSms(string phoneNumber, string message)
        {
            smsService.Send(phoneNumber, message);
        }
    }
}

