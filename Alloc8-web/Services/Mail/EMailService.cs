using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Mail.Model;

namespace Alloc8_web.Services.Mail
{
    public class EMailService:IEMailService
    {
        protected ISendGridClient _sendGridClient;
        protected EmailAddress _sender = new EmailAddress("hello@actualisation.ai", "Actualisation");
        public EMailService(ISendGridClient sendGridClient)
        {
            _sendGridClient = sendGridClient;
        }
        public async Task<bool> sendEmail(string? email,string? subject,string? html)
        {
            if(string.IsNullOrEmpty(email))
            {
                return false;
            }
            if(string.IsNullOrEmpty(subject))
            {
                return false;
            }
            if(string.IsNullOrEmpty(html))
            {
                return false;
            }
            var reciever = new EmailAddress(email, "");
            try
            {
                var msg = MailHelper.CreateSingleEmail(_sender, reciever, subject, null, html);
                var response = await _sendGridClient.SendEmailAsync(msg);
                if(response == null)
                {
                    return false;
                }
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending Email {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
