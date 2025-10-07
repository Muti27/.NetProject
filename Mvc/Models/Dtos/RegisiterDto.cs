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
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class DeleteDto
    {
        public int Id { get; set; }
    }
}
