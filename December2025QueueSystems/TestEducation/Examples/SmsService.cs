using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal class SmsService : ISmsService
    {
        private readonly ISmsService _smsService;

        public SmsService(ISmsService smsService)
        {
            _smsService = smsService;
        }

        public void Send(string phoneNumber, string message)
        {
            _smsService.Send(phoneNumber, message);
        }
    }
}

