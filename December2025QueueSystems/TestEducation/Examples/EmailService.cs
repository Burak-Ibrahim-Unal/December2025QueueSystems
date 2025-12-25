using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestEducation.Examples.Observer;

namespace TestEducation.Examples
{
    internal class EmailService : IEmailService, IUserObserver
    {
        private readonly IEmailService _emailService;

        public EmailService()
        {

        }


        public void Send(string to, string subject, string body)
        {
            _emailService.Send(to, subject, body);
        }

        public void ProcessOtherOperations()
        {
            Console.WriteLine("Email sent");
        }
    }
}

