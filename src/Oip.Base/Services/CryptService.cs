using Microsoft.AspNetCore.DataProtection;

namespace Oip.Base.Services;

/// <summary>
/// Provides encryption and decryption services using data protection APIs
/// </summary>
public class CryptService(IDataProtectionProvider dataProtectionProvider)
{
    private readonly IDataProtector _dataProtector = dataProtectionProvider.CreateProtector(nameof(CryptService));

    /// <summary>
    /// Encrypts a message using the data protection API
    /// </summary>
    /// <param name="message">The message to encrypt</param>
    /// <returns>The encrypted message</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string Protect(string? message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));
        return _dataProtector.Protect(message);
    }

    /// <summary>
    /// Decrypts a message using the data protection API
    /// </summary>
    /// <param name="message">The message to decrypt</param>
    /// <returns>The decrypted message</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string Unprotect(string? message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));
        return _dataProtector.Unprotect(message);
    }
}