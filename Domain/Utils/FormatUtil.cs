using System.Text.RegularExpressions;

namespace Domain.Utils
{
    public static class FormatUtil
    {
        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string password)
        {
            if (password != string.Empty && password != null)
            {
                if (!Regex.IsMatch(password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,})")) return false;

                var specialchar = new List<string> { "@", "#", "$", "%", "^", "&", "+", "=", ".", ",", "<", ">", "`", "!", "/", "?", "@", "\"", "'", "~", "\\", "[", "]", "{", "}", "*", "(", ")", "-", "+", "|", "_", "[", "]", "/", ":", ";" };
                if (!specialchar.Any(a => password.Contains(a))) return false;

                return true;
            }
            else return false;
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
