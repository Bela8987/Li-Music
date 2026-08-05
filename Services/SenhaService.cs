using System.Security.Cryptography;
using System.Text;

namespace LI_Music.Services;

public static class SenhaService
{
    public static string CriarHash(string senha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senha);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool Conferir(string senha, string hashEsperado)
    {
        if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(hashEsperado))
            return false;

        var hashRecebido = CriarHash(senha);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hashRecebido),
            Encoding.UTF8.GetBytes(hashEsperado));
    }
}
