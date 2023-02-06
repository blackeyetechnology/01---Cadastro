namespace Order.Domain.Models
{
    public class UserModel : EntityBase
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string ActiveUser { get; set; }
        public int VerificationCode { get; set; }
    }
}
