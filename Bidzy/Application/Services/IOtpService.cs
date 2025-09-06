namespace Bidzy.Application.Services
{
    public interface IOtpService
    {
        void StoreOtp(string email, string otp);
        bool ValidateOtp(string email, string inputOtp);
    }
}
