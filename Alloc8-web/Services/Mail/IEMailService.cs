namespace Alloc8_web.Services.Mail
{
    public interface IEMailService
    {
        public Task<bool> sendEmail(string? email, string? subject, string? html);
    }
}
