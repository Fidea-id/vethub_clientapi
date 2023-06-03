using Domain.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace Application.Utils
{
    public static class PasswordHashUtil
    {
        //public static bool VerifyPasswordHash(User user, string hashedPassword, string inputPassword)
        //{
        //    bool verified = true;
        //    if (!string.IsNullOrEmpty(hashedPassword))
        //    {
        //        var passwordHasher = new PasswordHasher<User>();
        //        var result = passwordHasher.VerifyHashedPassword(user, hashedPassword, inputPassword);

        //        if (result is PasswordVerificationResult.Failed) verified = false;
        //    }
        //    else verified = false;

        //    return verified;
        //}
    }
}
