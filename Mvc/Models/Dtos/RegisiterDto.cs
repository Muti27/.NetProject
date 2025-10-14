using System.ComponentModel.DataAnnotations;

namespace Mvc.Models.Dtos
{
    public class RegisiterDto
    {
        public string Username { get; set; }
        [Required] public string Email { get; set; }
        [StringLength(20, MinimumLength = 6)] public string Password { get; set; }
    }

    public class LoginDto
    {
        [Required] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }

    public class DeleteDto
    {
        public int Id { get; set; }
    }

    public class UsereProfileDto
    { 
        public string Email { get; set; }
        public string Username { get; set; }
        public ERole Role { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class ChangePasswordDto
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string newPasswordVaild { get; set; }
    }
}
