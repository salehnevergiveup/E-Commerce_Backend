using System.Security.Cryptography;
namespace PototoTrade.Service.Utilites.Hash;
public class Hashing : IHashing
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 256 / 8;
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
    private const char Delimiter = ';';

    public string Hash(string input)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, Iterations, _hashAlgorithmName, outputLength: KeySize);

        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string hash, string input)
    {
        var elements = hash.Split(Delimiter);
        var salt = Convert.FromBase64String(elements[0]);
        var hashed = Convert.FromBase64String(elements[1]);

        var hashInput = Rfc2898DeriveBytes.Pbkdf2(input, salt, Iterations, _hashAlgorithmName, KeySize);

        return CryptographicOperations.FixedTimeEquals(hashed, hashInput);
    }


}
