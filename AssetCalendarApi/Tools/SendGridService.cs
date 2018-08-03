using AssetCalendarApi.Data.Models;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Tools
{
    public class SendGridService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        public SendGridService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["SendGrid_API"];
        }

        public void SendEmailConfirmationEmail( AceUser user, string code)
        {
            var client = new SendGridClient(_apiKey);

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("registration@acecalendar.io", "Ace Calendar Team"),
                Subject = "Email Confirmation",

            };
        }

        private string GetEmailConfirmationEmailMessageHtml(AceUser user, string code)
        {
            return code;
        }
    }
}
