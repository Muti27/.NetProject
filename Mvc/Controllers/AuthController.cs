using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Dtos;
using Mvc.Models;
using System.Security.Claims;
using System.Threading.Tasks;

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

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("auth/Regisiter")]
        public async Task<IActionResult> Regisiter(RegisiterDto regisiterDto)
        {
            if (await dbContext.Users.AnyAsync(x => x.Email == regisiterDto.Email))
            {
                return BadRequest("");
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

            return Ok();
        }

        [HttpPost("auth/Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null)
            {
                return BadRequest();
            }

            var result = pwHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return BadRequest();
            }

            var token = JWTHelper.GenerateToken(user);

            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("auth/GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (id == null || email == null)
                return Unauthorized(new { message = "" });

            var user = await dbContext.Users.FindAsync(int.Parse(id));
            if (user == null)
                return NotFound();

            return Ok(new
            {
                id,                
                email,
                user.Username,
                user.CreateTime
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CheckPassword(string password)
        {


            return true;
        }
    }
}
