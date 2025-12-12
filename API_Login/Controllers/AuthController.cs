using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_Login.Services;
using Models;

namespace API_Login.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITaiKhoan_BLL _taiKhoanBll;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(ITaiKhoan_BLL taiKhoanBll, IJwtTokenService jwtTokenService)
        {
            _taiKhoanBll = taiKhoanBll;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _taiKhoanBll.DangNhapAsync(request.TenDangNhap, request.MatKhau);
            if (user == null)
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.TenDangNhap,
                    user.HoTen,
                    user.Quyen
                }
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                UserName = User.Identity?.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}
