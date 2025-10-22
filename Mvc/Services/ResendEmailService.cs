
using Microsoft.AspNetCore.Http;
using Resend;
using System.Net;
using System.Net.Mail;

namespace Mvc.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IResend resendClient;

        public ResendEmailService(IConfiguration config, IHttpContextAccessor accessor) 
        {
            httpContextAccessor = accessor;

            string apiKey = config["Resend:ApiKey"];
            resendClient = ResendClient.Create(apiKey);            
        }

        public async Task SendEmail(string email, string token, EmailType type)
        {
            //驗證url
            var request = httpContextAccessor?.HttpContext?.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            string confirmUrl = type switch
            {
                EmailType.EmailVerify => $"{baseUrl}/Auth/ConfirmEmail?email={email}&token={token}",
                EmailType.ForgetPassword => $"{baseUrl}/Auth/ResetPassword?email={email}&token={token}",
                _ => ""
            };

            string subString = string.Empty;
            string bodyString = string.Empty;

            EmailContent(type, out subString, out bodyString);

            var resp = new EmailMessage()
            {
                From = "onboarding@resend.dev",
                To = email,
                Subject = subString,
                HtmlBody = string.Format(bodyString, confirmUrl),
            };

            try
            {
                await resendClient.EmailSendAsync(resp);
            }
            catch (Exception ex)
            {
                throw new Exception($"寄送驗證信失敗，{ex}");
            }
        }

        private void EmailContent(EmailType type, out string sub, out string body)
        {
            sub = type switch
            {
                EmailType.EmailVerify => "請驗證您的電子信箱",
                EmailType.ForgetPassword => "請重新設定密碼",
                _ => "",
            };

            body = type switch
            {
                EmailType.EmailVerify => "請點擊以下連結完成驗證：\n{0}",
                EmailType.ForgetPassword => "請點擊以下連結重新設置密碼：\n{0}",
                _ => ""
            };
        }
    }
}
