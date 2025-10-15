using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Models;
using Mvc.Models.Dtos;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext dbContext;
        private readonly IPasswordHasher<User> pwHasher;

        public AuthController(AppDbContext db, IPasswordHasher<User> pwh)
        {
            dbContext = db;
            pwHasher = pwh;
        }

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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Regisiter(RegisiterDto regisiterDto)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (await dbContext.Users.AnyAsync(x => x.Email == regisiterDto.Email))
            {
                ViewBag.Error = "此信箱已經註冊過會員。";
                return View();
            }

            var user = new User
            {
                Username = regisiterDto.Username,
                Email = regisiterDto.Email,
                Role = ERole.User,
                CreateTime = DateTime.UtcNow
            };
            user.PasswordHash = pwHasher.HashPassword(user, regisiterDto.Password);

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null)
            {
                ViewBag.Error = "帳號或密碼輸入錯誤。";
                return View();
            }

            var result = pwHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "帳號或密碼輸入錯誤。";
                return View();
            }

            // 登入成功 -> 可以暫存Session
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserDisplayName", user.Username ?? user.Email);

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

            var user = await dbContext.Users.FindAsync(int.Parse(id));
            if (user == null)
            {
                return NotFound();
            }

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
            var users = await dbContext.Users.OrderBy(x => x.Id).ToListAsync();

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
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == deleteDto.Id);
            if (user == null)
                return NotFound();

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("ManagerUsers");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "輸入資料有誤。");
                return View(dto);
            }

            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null)
            {
                return NotFound();
            }

            var user = await dbContext.Users.FindAsync(int.Parse(id));
            if (user == null)
            {
                return NotFound();
            }

            var result = pwHasher.VerifyHashedPassword(user, user.PasswordHash, dto.oldPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("oldPassword", "舊密碼輸入錯誤。");
                return View();
            }
            else
            {
                if (dto.newPassword != dto.newPasswordVaild)
                {
                    ModelState.AddModelError("newPasswordVaild", "密碼驗證錯誤。");
                    return View();
                }

                user.PasswordHash = pwHasher.HashPassword(user, dto.newPassword);

                dbContext.Users.Update(user);
                await dbContext.SaveChangesAsync();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction("Login", "Auth");
            }
        }
    }
}
