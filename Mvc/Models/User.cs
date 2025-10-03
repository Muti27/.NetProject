namespace Mvc.Models
{
    public enum ERole
    {
        User,
        Admin,
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }      
        public DateTime CreateTime { get; set; }
        public ERole Role { get; set; }
    }
}
