using Entities.Models;
using Entities.Requests;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.Security.Cryptography;
using System.Text;

namespace MeetingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public UserController(IUserService userService, IConfiguration configuration, ITokenService tokenService)
        {
            _userService = userService;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public ActionResult<User> RegisterUser([FromBody] User user)
        {
            // ModelState geçerli mi kontrol et
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // E-posta ve şifre boş mu?
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Email and password cannot be empty.");
            }

            // E-posta formatı geçerli mi?
            if (!user.Email.Contains("@"))
            {
                return BadRequest("Invalid email format.");
            }

            // Şifre uzunluğu yeterli mi?
            if (user.Password.Length < 6)
            {
                return BadRequest("Password must be at least 6 characters long.");
            }

            // E-posta daha önce kaydedilmiş mi?
            var existingUser = _userService.GetUserByEmail(user.Email);
            if (existingUser != null && !existingUser.IsDeleted)
            {
                return Conflict("User with this email already exists.");
            }

            // Şifreyi hash'le
            user.Password = _userService.HashPassword(user.Password);

            // Yeni kullanıcıyı aktif olarak ekle
            user.IsDeleted = false;

            try
            {
                // Kullanıcıyı kaydet
                _userService.AddUser(user);
            }
            catch (Exception ex)
            {
                // Hata durumunda 500 yanıtı gönder
                return StatusCode(500, $"An error occurred while creating the user: {ex.Message}");
            }

            // Başarılı işlem, kullanıcıyı geri döndür
            return Ok(user);
        }



        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var existingUser = _userService.GetUserByEmail(loginRequest.Email);

            if (existingUser == null || existingUser.IsDeleted)
            {
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });
            }
            
            // Şifreyi doğrula
            if (!_userService.VerifyPassword(existingUser.Password, loginRequest.Password))
            {
                return Unauthorized(new { message = "Yanlış şifre." });
            }

            // Token oluştur
            var token = _tokenService.GenerateToken(existingUser);

            // Giriş başarılı olduğunda token'ı döndür
            return Ok(new
            {
                token = token,
                name = existingUser.Name,
                surname = existingUser.Surname,
                email = existingUser.Email
            });
        }

        [HttpPut("update")]
        public IActionResult UpdateUser([FromBody] UserUpdateDto userUpdateDto)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Unauthorized(new { message = "Token bulunamadı." });
                }

                var userId = _tokenService.ValidateTokenAndGetUserId(token);
                if (userId == null)
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var existingUser = _userService.GetUserById(userId.Value);
                if (existingUser == null || existingUser.IsDeleted)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                // Gelen DTO'daki alanlara göre güncelle
                if (!string.IsNullOrWhiteSpace(userUpdateDto.Name)) existingUser.Name = userUpdateDto.Name;
                if (!string.IsNullOrWhiteSpace(userUpdateDto.Surname)) existingUser.Surname = userUpdateDto.Surname;
                if (!string.IsNullOrWhiteSpace(userUpdateDto.Email)) existingUser.Email = userUpdateDto.Email;
                if (!string.IsNullOrWhiteSpace(userUpdateDto.Password))
                {
                    existingUser.Password = _userService.HashPassword(userUpdateDto.Password);
                }

                _userService.UpdateUser(existingUser);

                return Ok(new { message = "Kullanıcı bilgileri başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + ex.Message });
            }
        }

        [HttpDelete("softDelete/{id}")]
        public IActionResult SoftDeleteUser(int id)
        {
            try
            {
                var user = _userService.GetUserById(id);
                if (user == null || user.IsDeleted)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı veya zaten silinmiş." });
                }

                user.IsDeleted = true;
                _userService.UpdateUser(user);

                return Ok(new { message = "Kullanıcı başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + ex.Message });
            }
        }


        ////Token bilgilerini korntrol etmek için yazılan test endpointi
        //[HttpGet("tokenInfo")]
        //public IActionResult GetTokenInfo()
        //{
        //    try
        //    {
        //        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //        if (string.IsNullOrWhiteSpace(token))
        //        {
        //            return Unauthorized(new { message = "Token bulunamadı." });
        //        }

        //        var userId = _tokenService.ValidateTokenAndGetUserId(token);
        //        if (userId == null)
        //        {
        //            return Unauthorized(new { message = "Geçersiz token." });
        //        }

        //        var user = _userService.GetUserById(userId.Value);
        //        if (user == null || user.IsDeleted)
        //        {
        //            return NotFound(new { message = "Kullanıcı bulunamadı." });
        //        }

        //        var claims = new
        //        {
        //            user.Id,
        //            user.Name,
        //            user.Surname,
        //            user.Email
        //        };

        //        return Ok(new
        //        {
        //            message = "Token bilgileri başarıyla alındı.",
        //            claims = claims
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Sunucu hatası: " + ex.Message });
        //    }
        //}
    }
}
