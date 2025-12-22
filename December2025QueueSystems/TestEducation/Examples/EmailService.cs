using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal class EmailService(IEmailService emailService)
    {
        public void SendEmail(string to, string subject, string body)
        {
            emailService.Send(to, subject, body);
        }
    }
}

