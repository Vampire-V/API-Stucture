namespace Infrastructure.Utilities;

public static class RsaKeyPaths
{
    public static string PrivateKeyPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keys", "private.key");
    public static string PublicKeyPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keys", "public.key");
}
