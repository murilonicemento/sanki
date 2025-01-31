namespace Sanki.Services.Contracts;

public interface IPasswordService
{
    public byte[] GenerateSalt(byte[] salt);
    public string EncryptPassword(string password, byte[] salt);
}