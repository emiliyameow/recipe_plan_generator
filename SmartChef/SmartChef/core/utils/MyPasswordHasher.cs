using System.Security.Cryptography;

namespace SmartChef.core.utils;

public static class MyPasswordHasher
{
    private const int SaltSize = 16; // соль - случайная добавка к паролю, которая делает хеши уникальными даже при одинаковых паролях. длиной 128 байт
    private const int KeySize = 32; //итоговый хеш длиной 32 байта (256 бит).
    private const int Iterations = 10000; //количество итераций алгоритма PBKDF2
    private static readonly HashAlgorithmName HashAlgorithmName = HashAlgorithmName.SHA256;
    private const char SaltDelimeter = ';'; //символ, которым соль и хеш разделяются при сохранении в одну строку.
    
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize); //криптографически надёжный генератор случайных байт.
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName, KeySize); // вычисление хеша
        return string.Join(SaltDelimeter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }
    
    public static bool Validate(string passwordHash, string password)
    {
        var pwdElements = passwordHash.Split(SaltDelimeter); //разделяет
        var salt = Convert.FromBase64String(pwdElements[0]);
        var hash = Convert.FromBase64String(pwdElements[1]);
        var hashInput = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName, KeySize); // декодирует
        
        return CryptographicOperations.FixedTimeEquals(hash, hashInput);
    }
    
}