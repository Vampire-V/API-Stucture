using System.Security.Cryptography;
using Infrastructure.Utilities;

namespace Infrastructure.Services;

public class RsaKeyService
{
    private readonly string _privateKeyPath;
    private readonly string _publicKeyPath;

    public RsaKeyService()
    {
        _privateKeyPath = RsaKeyPaths.PrivateKeyPath;
        _publicKeyPath = RsaKeyPaths.PublicKeyPath;
    }

    /// <summary>
    /// ตรวจสอบและสร้าง Key หากยังไม่มีอยู่
    /// </summary>
    public void EnsureKeysExist()
    {
        if (!Directory.Exists(Path.GetDirectoryName(_privateKeyPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_privateKeyPath)!);
        }

        if (!File.Exists(_privateKeyPath) || !File.Exists(_publicKeyPath))
        {
            GenerateKeys();
        }
    }

    /// <summary>
    /// สร้างคู่ Private และ Public Key ใหม่
    /// </summary>
    public void GenerateKeys()
    {
        using var rsa = RSA.Create(2048);

        // Save Private Key
        File.WriteAllBytes(_privateKeyPath, rsa.ExportRSAPrivateKey());

        // Save Public Key
        File.WriteAllBytes(_publicKeyPath, rsa.ExportRSAPublicKey());
    }

    /// <summary>
    /// โหลด Private Key จากไฟล์
    /// </summary>
    /// <returns>RSA Object</returns>
    public RSA LoadPrivateKey()
    {
        if (!File.Exists(_privateKeyPath))
        {
            throw new FileNotFoundException(
                "Private key not found. EnsureKeysExist must be called first."
            );
        }

        var privateKeyBytes = File.ReadAllBytes(_privateKeyPath);
        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        return rsa;
    }

    /// <summary>
    /// โหลด Public Key จากไฟล์
    /// </summary>
    /// <returns>RSA Object</returns>
    public RSA LoadPublicKey()
    {
        if (!File.Exists(_publicKeyPath))
        {
            throw new FileNotFoundException(
                "Public key not found. EnsureKeysExist must be called first."
            );
        }

        var publicKeyBytes = File.ReadAllBytes(_publicKeyPath);
        var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKeyBytes, out _);
        return rsa;
    }

    /// <summary>
    /// ลบ Key ทั้งหมด
    /// </summary>
    public void DeleteKeys()
    {
        if (File.Exists(_privateKeyPath))
        {
            File.Delete(_privateKeyPath);
        }

        if (File.Exists(_publicKeyPath))
        {
            File.Delete(_publicKeyPath);
        }
    }
}
