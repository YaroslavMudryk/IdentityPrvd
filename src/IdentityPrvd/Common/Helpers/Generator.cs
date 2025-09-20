using System.Security.Cryptography;

namespace IdentityPrvd.Common.Helpers;

public class Generator
{
    private static string _upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static string _lowerChars = "abcdefghijklmnopqrstuvwxyz";
    private static string _numbersChars = "0123456789";
    private static string _chars = $"{_upperChars}{_numbersChars}{_lowerChars}";

    public static string GetAdminEmail()
    {
        return "admin@mailsecret.com";
    }

    public static string GetAdminPhone()
    {
        return "+10000000000";
    }

    public static string GetPassword()
    {
        var random = new Random();
        var passwordChars = new char[12];
        // Ensure at least one character from each category
        passwordChars[0] = _upperChars[random.Next(_upperChars.Length)];
        passwordChars[1] = _lowerChars[random.Next(_lowerChars.Length)];
        passwordChars[2] = _numbersChars[random.Next(_numbersChars.Length)];
        // Fill the remaining characters randomly
        for (int i = 3; i < passwordChars.Length; i++)
        {
            passwordChars[i] = _chars[random.Next(_chars.Length)];
        }
        // Shuffle the characters to avoid predictable patterns
        return new string(passwordChars.OrderBy(x => random.Next()).ToArray());
    }

    public static string CreateAppId()
    {
        return GetUniqCode(4);
    }

    public static string CreateAppSecret()
    {
        return GetString(70);
    }

    public static string GetUsername()
    {
        string username;
        username = GetString(12);
        return username;
    }

    public static string GetStringId()
    {
        long ticks = DateTime.UtcNow.Ticks;
        byte[] bytes = BitConverter.GetBytes(ticks);
        string id = Convert.ToBase64String(bytes)
                                .Replace('+', '_')
                                .Replace('/', '-')
                                .TrimEnd('=');
        return id;
    }

    public static Ulid GetSessionId()
    {
        return Ulid.NewUlid();
    }

    public static string GetRefreshToken()
    {
        return $"{Guid.CreateVersion7():N}{Guid.CreateVersion7():N}";
    }

    public static string GetSessionVerification()
    {
        return Ulid.NewUlid().ToString();
    }

    public static string GetUniqCode(int sections)
    {
        var commonWords = sections * 4;
        var commonCountOfSymbols = commonWords + (sections - 1);
        var stringChars = new char[commonCountOfSymbols];
        var positons = GetHyphenPositions(sections);
        var random = new Random();
        for (int i = 0; i < stringChars.Length; i++)
        {
            if (positons.Contains(i))
            {
                stringChars[i] = '-';
                continue;
            }
            stringChars[i] = _chars[random.Next(_chars.Length)];
        }
        return new String(stringChars);
    }

    public static string GetString(int length, bool IsUpper = false, bool IsLower = false)
    {
        var stringChars = new char[length];
        var random = new Random();
        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = _chars[random.Next(_chars.Length)];
        }
        var result = new string(stringChars);
        return IsUpper ? result.ToUpper() : IsLower ? result.ToLower() : result;
    }

    public static string GetCode(int size)
    {
        return Random.Shared.Next(10000000, 99999999).ToString();
    }

    public static string[] GetRestoreCodes()
    {
        return
        [
            Guid.NewGuid().ToString("N")[..10],
            Guid.NewGuid().ToString("N")[..10],
            Guid.NewGuid().ToString("N")[..10],
            Guid.NewGuid().ToString("N")[..10]
        ];
    }

    public static IEnumerable<string> GetRecoveryCodes()
    {
        var codes = new List<string>();

        using (var rng = RandomNumberGenerator.Create())
        {
            for (int i = 0; i < 10; i++)
            {
                var code = GenerateCode(rng);
                codes.Add(code);
            }
        }

        return codes;
    }

    private static string GenerateCode(RandomNumberGenerator rng)
    {
        // Exclude confusing characters: 0, O, I, 1, etc.
        const string chars = "ACDEFGHJKLMNPQRSTUVWXYZ23456789";
        var buffer = new byte[10];
        rng.GetBytes(buffer);

        var result = new char[10];
        for (int i = 0; i < 10; i++)
        {
            result[i] = chars[buffer[i] % chars.Length];
        }

        return new string(result);
    }

    private static int[] GetHyphenPositions(int sections)
    {
        var pos = new int[sections - 1];
        var baseIndex = 4;
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = baseIndex;
            baseIndex += 4 + 1;
        }
        return pos;
    }
}
