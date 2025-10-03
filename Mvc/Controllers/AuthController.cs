using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Dtos;
using Mvc.Models;
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
                Role = ERole.User
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
        public IActionResult GetProfile()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);            
            var name = User.FindFirstValue(ClaimTypes.Name);

            if (id == null || email == null || name == null)
                return Unauthorized(new { message = "" });
                                    
            return Ok(new
            {
                Id = id,
                Username = name,
                Email = email
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
