using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Sanki.Services.Contracts;

namespace Sanki.Services;

public class PasswordService : IPasswordService
{
    public byte[] GenerateSalt(byte[] salt)
    {
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        return salt;
    }

    public string EncryptPassword(string password, byte[] salt)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8)
        );
    }
}