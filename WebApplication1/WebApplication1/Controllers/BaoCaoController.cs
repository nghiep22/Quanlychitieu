using Microsoft.AspNetCore.Mvc;
using QL_NganQuy.DTO;
using QL_NganQuy.Services;

namespace QL_NganQuy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaoCaoController : ControllerBase
    {
        private readonly IBaoCaoService _service;

        public BaoCaoController(IBaoCaoService service)
        {
            _service = service;
        }

        // POST: api/BaoCao/tong-quan
        [HttpPost("tong-quan")]
        public IActionResult GetTongQuanThuChi([FromBody] BaoCaoFilterDto filter)
        {
            try
            {
                var result = _service.GetTongQuanThuChi(filter);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/BaoCao/theo-danh-muc
        [HttpPost("theo-danh-muc")]
        public IActionResult GetBaoCaoTheoDanhMuc([FromBody] BaoCaoFilterDto filter)
        {
            try
            {
                var result = _service.GetBaoCaoTheoDanhMuc(filter);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/BaoCao/theo-vi/{taiKhoanId}
        [HttpGet("theo-vi/{taiKhoanId}")]
        public IActionResult GetBaoCaoTheoVi(int taiKhoanId, [FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            try
            {
                var result = _service.GetBaoCaoTheoVi(taiKhoanId, tuNgay, denNgay);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/BaoCao/theo-ngay/{taiKhoanId}
        [HttpGet("theo-ngay/{taiKhoanId}")]
        public IActionResult GetBaoCaoTheoNgay(int taiKhoanId, [FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            try
            {
                var result = _service.GetBaoCaoTheoNgay(taiKhoanId, tuNgay, denNgay);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/BaoCao/theo-thang/{taiKhoanId}/{nam}
        [HttpGet("theo-thang/{taiKhoanId}/{nam}")]
        public IActionResult GetBaoCaoTheoThang(int taiKhoanId, int nam)
        {
            try
            {
                var result = _service.GetBaoCaoTheoThang(taiKhoanId, nam);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/BaoCao/so-sanh-ngan-sach/{taiKhoanId}
        [HttpGet("so-sanh-ngan-sach/{taiKhoanId}")]
        public IActionResult GetSoSanhNganSach(int taiKhoanId, [FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            try
            {
                var result = _service.GetSoSanhNganSach(taiKhoanId, tuNgay, denNgay);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/BaoCao/top-giao-dich/{taiKhoanId}
        [HttpGet("top-giao-dich/{taiKhoanId}")]
        public IActionResult GetTopGiaoDich(int taiKhoanId, [FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay, [FromQuery] string loaiGD = "", [FromQuery] int top = 10)
        {
            try
            {
                var result = _service.GetTopGiaoDich(taiKhoanId, tuNgay, denNgay, loaiGD, top);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/BaoCao/tong-hop/{taiKhoanId}
        [HttpGet("tong-hop/{taiKhoanId}")]
        public IActionResult GetBaoCaoTongHop(int taiKhoanId, [FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            try
            {
                var result = _service.GetBaoCaoTongHop(taiKhoanId, tuNgay, denNgay);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}