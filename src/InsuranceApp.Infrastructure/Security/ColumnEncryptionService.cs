using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Security;

public sealed class ColumnEncryptionOptions
{
    /// 32-byte (256-bit) key as base64. If empty, no encryption is applied (dev fallback).
    public string KeyBase64 { get; set; } = string.Empty;
}

public sealed class ColumnEncryptionService(IOptions<ColumnEncryptionOptions> options)
{
    private const string Prefix = "enc:";
    private readonly byte[] _key = ResolveKey(options.Value.KeyBase64);

    private static byte[] ResolveKey(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return Array.Empty<byte>();
        var bytes = Convert.FromBase64String(base64);
        if (bytes.Length != 32) throw new InvalidOperationException("ColumnEncryption key must be 32 bytes (base64).");
        return bytes;
    }

    public bool IsEnabled => _key.Length == 32;

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext) || !IsEnabled || plaintext.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return plaintext ?? string.Empty;
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var cipher = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length);
        var combined = new byte[aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, combined, aes.IV.Length, cipher.Length);
        return Prefix + Convert.ToBase64String(combined);
    }

    public string Decrypt(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith(Prefix, StringComparison.Ordinal) || !IsEnabled)
        {
            return value ?? string.Empty;
        }

        var combined = Convert.FromBase64String(value[Prefix.Length..]);
        using var aes = Aes.Create();
        aes.Key = _key;
        var iv = combined[..16];
        var cipher = combined[16..];
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }
}

public sealed class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(ColumnEncryptionService cipher)
        : base(v => cipher.Encrypt(v ?? string.Empty), v => cipher.Decrypt(v ?? string.Empty))
    {
    }
}
