using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Mvc.Models;
using Mvc.Repository;
using System.Linq;

namespace Mvc.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<User>> Login(string email, string password);
        Task<ServiceResult<User>> Regisiter(string username, string email, string password, string passwordVerify);
        Task<ServiceResult<User>> DeleteUser(int id);
        Task<ServiceResult<User>> ChangePassword(string email, string oldPassword, string newPassword, string newPasswordVaild);
        Task<ServiceResult<User>> ResetPassword(string email, string token, string newPassword, string newPasswordVaild);
        Task<ServiceResult<User>> GetUser(int id);
        Task<List<User>> GetUserList();
        Task<ServiceResult<User>> ConfirmEmail(string email, string token);
        Task<ServiceResult<User>> ReSendConfirmEmail(string email);
        Task<ServiceResult<User>> ForgetPassword(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository userRepository;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IEmailService emailService;
        private readonly IMemoryCache memoryCache;

        public AuthService(IUserRepository userRepo, IPasswordHasher<User> pwh, IEmailService email, IMemoryCache cache)
        {
            userRepository = userRepo;
            passwordHasher = pwh;
            emailService = email;
            memoryCache = cache;
        }

        public async Task<ServiceResult<User>> GetUser(int id)
        {
            var user = await userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return new ServiceResult<User>
                {
                    Success = false,
                    Message = "找不到此使用者資料。"
                };
            }

            return new ServiceResult<User>
            {
                Success = true,
                Data = user
            };
        }

        public async Task<List<User>> GetUserList()
        {
            var list = await userRepository.GetAllAsync();

            return list.ToList();
        }

        public async Task<ServiceResult<User>> Login(string email, string password)
        {
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return new ServiceResult<User>
                {
                    Success = false,
                    Message = "帳號或密碼輸入錯誤。"
                };
            }

            var passVaild = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (passVaild == PasswordVerificationResult.Failed)
            {
                return new ServiceResult<User>
                {
                    Success = false,
                    Message = "帳號或密碼輸入錯誤。"
                };
            }

            if (!user.IsApprovedEmail)
            {
                return ServiceResult<User>.Failed("此信箱尚未完成驗證。");
            }

            return new ServiceResult<User>
            {
                Success = true,
                Data = user
            };
        }

        public async Task<ServiceResult<User>> Regisiter(string username, string email, string password, string passwordVerify)
        {
            var exists = await userRepository.GetByEmailAsync(email);
            if (exists != null)
            {
                return ServiceResult<User>.Failed("此信箱已經註冊過會員。");
            }

            if (password != passwordVerify)
            {
                return ServiceResult<User>.Failed("密碼驗證錯誤");
            }

            var user = new User
            {
                Username = username ?? "新會員",
                Email = email,
                Role = ERole.User,
                CreateTime = DateTime.UtcNow,
                IsApprovedEmail = false
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);

            await userRepository.AddAsync(user);

            //發驗證信           
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            await emailService.SendEmail(email, token, EmailType.EmailVerify);

            memoryCache.Set($"email-confirm={email}", token, TimeSpan.FromHours(24));

            return new ServiceResult<User>
            {
                Success = true,
                Data = user
            };
        }

        public async Task<ServiceResult<User>> DeleteUser(int id)
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return new ServiceResult<User>
                {
                    Success = false,
                    Message = "找不到此會員。"
                };
            }

            await userRepository.DeleteAsync(user);

            return new ServiceResult<User>
            {
                Success = true
            };
        }

        public async Task<ServiceResult<User>> ChangePassword(string email, string oldPassword, string newPassword, string newPasswordVaild)
        {
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return new ServiceResult<User>
                {
                    Success = false,
                    Message = "找不到此會員。"
                };
            }

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                return new ServiceResult<User>
                {
                    Success = false,
                    Message = "舊密碼輸入錯誤。"
                };
            }
            else
            {
                if (newPassword != newPasswordVaild)
                {
                    return new ServiceResult<User>
                    {
                        Success = false,
                        Message = "密碼驗證錯誤。"
                    };
                }

                user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

                await userRepository.UpdateAsync(user);

                return new ServiceResult<User>
                {
                    Success = true
                };
            }
        }

        public async Task<ServiceResult<User>> ResetPassword(string email, string token, string newPassword, string newPasswordVerify)
        {
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ServiceResult<User>.Failed("找不到此會員。");
            }

            var cacheToken = memoryCache.Get<string>($"password-forget={email}");
            if (cacheToken == null || cacheToken != token)
            {
                return ServiceResult<User>.Failed("驗證信已失效，請重新驗證。");
            }

            if (newPassword != newPasswordVerify)
            {
                return ServiceResult<User>.Failed("密碼驗證錯誤。");
            }

            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

            await userRepository.UpdateAsync(user);

            return ServiceResult<User>.Ok(user);
        }

        public async Task<ServiceResult<User>> ConfirmEmail(string email, string token)
        {
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ServiceResult<User>.Failed("找不到使用者。");
            }

            //cache
            var cacheToken = memoryCache.Get<string>($"email-confirm={email}");
            if (cacheToken == null || cacheToken != token)
            {
                return ServiceResult<User>.Failed("驗證信已失效，請重新驗證。");
            }

            user.IsApprovedEmail = true;
            await userRepository.UpdateAsync(user);

            return ServiceResult<User>.Ok(user);
        }

        public async Task<ServiceResult<User>> ReSendConfirmEmail(string email)
        {
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ServiceResult<User>.Failed("");
            }

            //發驗證信           
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            await emailService.SendEmail(email, token, EmailType.EmailVerify);

            memoryCache.Set($"email-confirm={email}", token, TimeSpan.FromHours(24));

            return ServiceResult<User>.Ok(null);
        }

        public async Task<ServiceResult<User>> ForgetPassword(string email)
        {
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ServiceResult<User>.Failed("");
            }

            //發驗證信           
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            await emailService.SendEmail(email, token, EmailType.ForgetPassword);

            memoryCache.Set($"password-forget={email}", token, TimeSpan.FromHours(24));

            return ServiceResult<User>.Ok(null);
        }
    }
}
