// Hum .NET ki built-in cryptography library ko import kar rahe hain.
using System.Security.Cryptography;

namespace IndasApp.API.Services.Helpers
{
    // Yeh ek interface hai jo PasswordHasher class ke "contract" ko define karta hai.
    // Isse hum Dependency Injection aasaani se kar payenge.
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string hashedPassword, string providedPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        // Yeh constants hashing algorithm ke parameters hain.
        // Inhe change karne ki zaroorat nahi hai, yeh secure defaults hain.
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        private const char Delimiter = ';';

        // Yeh method ek plain text password ko ek secure hash me convert karega.
        public string Hash(string password)
        {
            // 1. Ek random "salt" generate karte hain.
            // Salt ek random data hota hai jo har password hash ko unique banata hai,
            // bhale hi do users ka password same ho.
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            // 2. PBKDF2 algorithm ka istemaal karke hash generate karte hain.
            // Yeh password, salt, aur iterations ko combine karke ek secure key (hash) banata hai.
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

            // 3. Hash aur salt ko ek saath ek hi string me store karte hain.
            // Hum unhe base64 me convert karte hain taaki woh readable text ban jayein.
            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        // Yeh method check karega ki user ka diya hua password, database me store kiye gaye hash se match karta hai ya nahi.
        public bool Verify(string hashedPassword, string providedPassword)
        {
            // 1. Stored string ko wapas salt aur hash me split karte hain.
            var parts = hashedPassword.Split(Delimiter);
            if (parts.Length != 2)
            {
                // Agar format sahi nahi hai, to yeh valid hash nahi hai.
                throw new FormatException("Unexpected hash format.");
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            // 2. User ke diye hue password se usi salt aur parameters ka istemaal karke ek naya hash generate karte hain.
            var newHash = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, Iterations, Algorithm, KeySize);

            // 3. Naye generate hue hash ko database se aaye hue hash se compare karte hain.
            // Humein ek special "CryptographicOperations.FixedTimeEquals" method use karna chahiye
            // taaki "timing attacks" se bacha ja sake.
            return CryptographicOperations.FixedTimeEquals(hash, newHash);
        }
    }
}