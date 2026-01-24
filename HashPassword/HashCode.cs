using System.Security.Cryptography;

namespace NotesApp.HashPassword
{
    public class HashCode
    {
        public string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rgb = RandomNumberGenerator.Create())
            {
                rgb.GetBytes(salt);
            }
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            byte[] hashCode = new byte[48];
            Array.Copy(salt, 0, hashCode, 0, 16);
            Array.Copy(hash, 0, hashCode, 16, hash.Length);
            return Convert.ToBase64String(hashCode);
        }
        public bool VereficationPassword(string password, string hashStand)
        {
            byte[] hashCode = Convert.FromBase64String(hashStand);
            byte[] salt = new byte[16];
            Array.Copy(hashCode, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashCode[i + 16] != hash[i])
                    return false;
            }
            return true;
        }
    }
}
