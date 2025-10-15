using Humanizer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mvc.Models;
using Mvc.Repository;

namespace Mvc.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<User>> Login(string email, string password);
        Task<ServiceResult<User>> Regisiter(string username, string email, string password);
        Task<ServiceResult<User>> DeleteUser(int id);
        Task<ServiceResult<User>> ChangePassword(int id, string oldPassword, string newPassword, string newPasswordVaild);
        Task<ServiceResult<User>> GetUser(int id);
        Task<List<User>> GetUserList();
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository userRepository;
        private readonly IPasswordHasher<User> passwordHasher;

        public AuthService(IUserRepository userRepo, IPasswordHasher<User> pwh)
        {
            userRepository = userRepo;
            passwordHasher = pwh;
        }

        public async Task<ServiceResult<User>> GetUser(int id)
        {
            var user = await userRepository.GetUserByIdAsync(id);

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
            var list = await userRepository.GetUserList();

            return list;
        }

        public async Task<ServiceResult<User>> Login(string email, string password)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
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

            return new ServiceResult<User>
            {
                Success = true,
                Data = user
            };
        }

        public async Task<ServiceResult<User>> Regisiter(string username, string email, string password)
        {
            var exists = await userRepository.GetUserByEmailAsync(email);
            if (exists != null)
            {
                return new ServiceResult<User>
                {
                    Success = false,
                    Message = "此信箱已經註冊過會員。"
                };
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Role = ERole.User,
                CreateTime = DateTime.UtcNow
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);

            await userRepository.AddAsync(user);

            return new ServiceResult<User>
            {
                Success = true,
                Data = user
            };
        }

        public async Task<ServiceResult<User>> DeleteUser(int id)
        {
            var user = await userRepository.GetUserByIdAsync(id);
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

        public async Task<ServiceResult<User>> ChangePassword(int id, string oldPassword, string newPassword, string newPasswordVaild)
        {
            var user = await userRepository.GetUserByIdAsync(id);
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
                //ModelState.AddModelError("oldPassword", "舊密碼輸入錯誤。");
                //return View();
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
                    //ModelState.AddModelError("newPasswordVaild", "密碼驗證錯誤。");
                    //return View();
                }

                user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

                await userRepository.UpdateAsync(user);

                return new ServiceResult<User>
                {
                    Success = true
                };
            }
        }
    }
}
