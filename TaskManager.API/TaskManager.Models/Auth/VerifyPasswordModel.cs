namespace TaskManager.Models.Auth
{
    public class VerifyPasswordModel
    {
        public required string Password { get; set; }
        public required string StoredHash { get; set; }
        public required string StoredSalt { get; set; }
    }
}