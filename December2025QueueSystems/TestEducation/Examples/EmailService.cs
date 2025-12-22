using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal class EmailService : IEmailService
    {
        private readonly IEmailService _emailService;

        public EmailService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public void Send(string to, string subject, string body)
        {
            _emailService.Send(to, subject, body);
        }
    }
}

