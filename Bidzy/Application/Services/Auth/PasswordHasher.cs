namespace Bidzy.Application.Services.Auth
{
    using BCrypt.Net;

    public static class PasswordHasher
    {
        public static bool Verify(string plainPassword, string hashedPassword)
        {
            return BCrypt.Verify(plainPassword, hashedPassword);
        }

        public static string Hash(string plainPassword)
        {
            return BCrypt.HashPassword(plainPassword);
        }
    }


}
