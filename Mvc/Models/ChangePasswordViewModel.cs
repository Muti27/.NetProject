namespace Mvc.Models
{
    public class ChangePasswordViewModel
    {
        public string? email { get; set; }
        public string? oldPassword { get; set; }
        public string newPassword { get; set; }
        public string newPasswordVerify{ get; set; }
        public string? token { get; set; }
        public bool isResetMode { get; set; } = false;
    }
}
