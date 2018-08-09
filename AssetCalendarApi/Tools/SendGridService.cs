using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AssetCalendarApi.Tools
{
    public class SendGridService
    {
        private readonly IConfiguration _configuration;
        private readonly RazorService _razorService;

        private readonly string _apiKey;

        public SendGridService(IConfiguration configuration, RazorService razorService)
        {
            _configuration = configuration;
            _apiKey = _configuration["SendGrid_API"];
            _razorService = razorService;
        }

        public void SendEmailConfirmationEmail( AceUser user, string code)
        {
            var client = new SendGridClient(_apiKey);

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("registration@acecalendar.io", "Ace Calendar Team"),
                Subject = "Email Confirmation",
                HtmlContent = GetEmailConfirmationEmailMessageHtml(user, code).Result,
            };

            msg.AddTo(user.Email);
            client.SendEmailAsync(msg);
        }

        private async Task<string> GetEmailConfirmationEmailMessageHtml(AceUser user, string code)
        {
            var model = new ConfirmEmailViewModel() { Username = user.UserName, Code = code };

            return await _razorService.RenderView("ConfirmEmailView", model);
        }
    }
}
