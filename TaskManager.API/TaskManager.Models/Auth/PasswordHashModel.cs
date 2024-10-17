namespace TaskManager.Models.Auth
{
    public class PasswordHashModel
    {
        public required string Hash { get; set; }
        public required string Salt { get; set; }
    }
}