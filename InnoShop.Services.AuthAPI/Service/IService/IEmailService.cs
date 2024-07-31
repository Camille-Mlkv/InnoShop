namespace InnoShop.Services.AuthAPI.Service.IService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
