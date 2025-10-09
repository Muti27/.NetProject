using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Models;
using Mvc.Models.Dtos;

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

        [HttpPost]
        public async Task<IActionResult> Regisiter(RegisiterDto regisiterDto)
        {
            if (await dbContext.Users.AnyAsync(x => x.Email == regisiterDto.Email))
            {
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
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null)
            {
                ViewBag.Error = "此信箱已註冊。";
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

            var token = JWTHelper.GenerateToken(user);

            return RedirectToAction("Index", "Home", routeValues: token);
        }

        //[Authorize]
        //[HttpGet("GetProfile")]
        //public async Task<IActionResult> GetProfile()
        //{
        //    var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var email = User.FindFirstValue(ClaimTypes.Email);

        //    if (id == null || email == null)
        //        return Unauthorized(new { message = "" });

        //    var user = await dbContext.Users.FindAsync(int.Parse(id));
        //    if (user == null)
        //        return NotFound();

        //    return Ok(new
        //    {
        //        id,
        //        email,
        //        user.Username,
        //        user.CreateTime
        //    });
        //}

        ////[Authorize(Roles = "Admin")]
        //[HttpPost("Delete")]
        //public async Task<IActionResult> DeleteUser(DeleteDto deleteDto)
        //{
        //    var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == deleteDto.Id);
        //    if (user == null)
        //        return NotFound();

        //    dbContext.Users.Remove(user);
        //    await dbContext.SaveChangesAsync();

        //    return Ok();
        //}
    }
}
