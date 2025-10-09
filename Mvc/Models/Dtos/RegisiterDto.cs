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
}
