using Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService : ITokenService
{
    IConfiguration _configuration;
    IUserService _userService;

    public TokenService(IConfiguration configuration, IUserService userService)
    {
        _configuration = configuration;
        _userService = userService;
    }

    public string GenerateToken(User user)
    {
        // Secret key ve JWT ayarlarını al
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        // Kullanıcı bilgilerini token'a ekle
        var Claims = new[]
        {
        new Claim(ClaimTypes.Name, user.Name),      
        new Claim(ClaimTypes.Surname, user.Surname), 
        new Claim(ClaimTypes.Email, user.Email),     
        new Claim("userId", user.Id.ToString()),     
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
    };

        // Token imzası
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        // Token özellikleri
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(Claims),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials,
            Expires = DateTime.Now.AddHours(1)  // Token geçerliliği 1 saat
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token); // Token'ı string olarak döndür
    }


    public int? ValidateTokenAndGetUserId(string token)
    {
        try
        {
            // Token'ı manuel olarak ayrıştır
            var claims = ParseJwt(token);
            if (claims != null && claims.TryGetValue("userId", out var userIdClaim))
            {
                // userIdClaim string olarak alındı ve integer'a çevriliyor
                if (!string.IsNullOrEmpty(userIdClaim?.ToString()) && int.TryParse(userIdClaim.ToString(), out int userId))
                {
                    // userId veritabanındaki kullanıcıyla eşleştiriliyor
                    var user = _userService.GetUserById(userId);
                    if (user != null)
                    {
                        // Kullanıcı bulundu, token geçerli
                        return userId;
                    }
                }
            }


            // Token veya kullanıcı doğrulaması başarısızsa null döner
            Console.WriteLine("Token doğrulama başarısız: userId bulunamadı veya geçersiz.");
            return null;
        }
        catch (Exception ex)
        {
            // Hata oluşursa loglama yapabilir ve null dönebilirsiniz
            Console.WriteLine($"Token doğrulama hatası: {ex.Message}");
            return null;
        }
    }

    // JWT'yi manuel olarak ayrıştırmak için yardımcı metod
    private IDictionary<string, object> ParseJwt(string token)
    {
        try
        {
            // Token'ı '.' ile ayır (header, payload, signature)
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Geçersiz JWT formatı.");
            }

            // Payload kısmını Base64'ten çöz
            var payload = parts[1];
            var decodedBytes = DecodeBase64(payload);
            var json = Encoding.UTF8.GetString(decodedBytes);

            // JSON'u bir dictionary'e çevir
            var claims = Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string, object>>(json);

            if (claims == null)
            {
                throw new Exception("Payload içeriği ayrıştırılamadı.");
            }

            return claims;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JWT ayrıştırma hatası: {ex.Message}");
            return null;
        }
    }

    // Base64 çözümleme metodunu iyileştirdik
    private byte[] DecodeBase64(string base64)
    {
        try
        {
            // Padding'i tamamla ve Base64 çözümle
            base64 = PadBase64String(base64);
            return Convert.FromBase64String(base64);
        }
        catch (FormatException ex)
        {
            throw new Exception($"Base64 format hatası: {ex.Message}");
        }
    }

    // Base64 padding'i tamamlamak için yardımcı metod
    private string PadBase64String(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: return base64 + "==";
            case 3: return base64 + "=";
            default: return base64;
        }
    }



}
