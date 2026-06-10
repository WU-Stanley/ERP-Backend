using System.Security.Cryptography;
using BCrypt.Net;
using WUIAM.Models;

namespace WUIAM.Services.Config
{
    public class PasswordUtilService
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        internal static string GenerateTwoFactorToken()
        {
            // Generate a random 6-digit token
            Random random = new Random();
            int token = random.Next(100000, 999999);
            return token.ToString();
        }
        public static RefreshToken GenerateRefreshToken(User user)
        {
            return (new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = user.Id,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)

            }
            );
        }
    }
}
