using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mvc.Models;
using Mvc.Models.Dtos;
using Mvc.Services;
using System.Security.Claims;

namespace Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService auth)
        {
            authService = auth;
        }

        #region View
        public IActionResult Regisiter()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ChangePasswordViewModel()
            {
                email = email,
                token = token,
                isResetMode = true,
            };
            return View("ChangePassword", model);
        }

        public IActionResult EmailVerify()
        {
            return View();
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> Regisiter(RegisiterDto regisiterDto)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var result = await authService.Regisiter(regisiterDto.Username, regisiterDto.Email, regisiterDto.Password, regisiterDto.PasswordVerify);
            if (result.Success == false)
            {
                ViewBag.Error = result.Message;
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var result = await authService.Login(loginDto.Email, loginDto.Password);
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View();
            }

            // 登入成功 -> 可以暫存Session
            //HttpContext.Session.SetString("UserEmail", user.Email);
            //HttpContext.Session.SetString("UserDisplayName", user.Username ?? user.Email);

            var user = result.Data;
#if UseJWT
            var token = JWTHelper.GenerateToken(user);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
#else           
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),                
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            //寫入 cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
#endif

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null)
            {                
                return RedirectToAction("Index", "Home");
            }

            var result = await authService.GetUser(int.Parse(id));
            if (!result.Success)
            {
                return NotFound();
            }

            var user = result.Data;

            var profileDto = new UsereProfileDto
            {
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                CreateTime = user.CreateTime.ToLocalTime()
            };

            return View(profileDto);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManagerUsers()
        {           
            List<User> users = await authService.GetUserList();

            foreach (var user in users)
            {
                user.CreateTime = user.CreateTime.ToLocalTime();
            }

            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(DeleteDto deleteDto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "輸入資料有誤。");
                return View(deleteDto);
            }

            var result = await authService.DeleteUser(deleteDto.Id);
            if (!result.Success)
            {
                return NotFound();
            }

            return RedirectToAction("ManagerUsers");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "輸入資料有誤。");
                return View(viewModel);
            }

            if (viewModel.isResetMode)
            {
                var email = viewModel.email;
                var token = viewModel.token;

                ChangePasswordDto dto = new ChangePasswordDto()
                {
                    newPassword = viewModel.newPassword,
                    newPasswordVerify = viewModel.newPasswordVerify
                };

                var result = await authService.ResetPassword(email, token, dto.newPassword, dto.newPasswordVerify);
                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message);
                    return View(viewModel);
                }
            }
            else
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (email == null)
                {
                    return NotFound();
                }

                ChangePasswordDto dto = new ChangePasswordDto()
                {
                    oldPassword = viewModel.oldPassword,
                    newPassword = viewModel.newPassword,
                    newPasswordVerify = viewModel.newPasswordVerify
                };

                var result = await authService.ChangePassword(email, dto.oldPassword, dto.newPassword, dto.newPasswordVerify);
                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message);
                    return View(viewModel);
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "輸入資料有誤。");
                return RedirectToAction("Index", "Home");
            }

            var result = await authService.ConfirmEmail(email, token);
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> EmailVerify(EmailVerifyDto dto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "輸入資料有誤。");
                return View();
            }            

            var result = await authService.ReSendConfirmEmail(dto.Email);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(EmailVerifyDto dto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "輸入資料有誤。");
                return View();
            }

            var result = await authService.ForgetPassword(dto.Email);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View();
            }

            return RedirectToAction("Login");
        }
    }
}
