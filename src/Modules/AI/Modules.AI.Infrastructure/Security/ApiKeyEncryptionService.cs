using Microsoft.AspNetCore.DataProtection;
using Modules.AI.Core.Security;

namespace Modules.AI.Infrastructure.Security;

public class ApiKeyEncryptionService(IDataProtectionProvider dataProtectionProvider) : IApiKeyEncryptionService
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("Quizzer.AI.ApiKeys");

    public string Encrypt(string plainTextKey)
    {
        return _protector.Protect(plainTextKey);
    }

    public string Decrypt(string encryptedKey)
    {
        return _protector.Unprotect(encryptedKey);
    }
}
