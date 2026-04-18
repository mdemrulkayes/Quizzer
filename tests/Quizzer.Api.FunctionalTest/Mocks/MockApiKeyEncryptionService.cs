using Modules.AI.Core.Security;

namespace Quizzer.Api.FunctionalTest.Mocks;

public class MockApiKeyEncryptionService : IApiKeyEncryptionService
{
    public string Encrypt(string plainTextKey) => $"encrypted:{plainTextKey}";
    public string Decrypt(string encryptedKey) => encryptedKey.Replace("encrypted:", "");
}
