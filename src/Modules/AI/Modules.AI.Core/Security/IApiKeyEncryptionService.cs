namespace Modules.AI.Core.Security;

public interface IApiKeyEncryptionService
{
    string Encrypt(string plainTextKey);
    string Decrypt(string encryptedKey);
}
