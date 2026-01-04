using Microsoft.AspNetCore.Mvc;
using QL_NganQuy.DTO;
using QL_NganQuy.Services;

namespace QL_NganQuy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongBaoController : ControllerBase
    {
        private readonly IThongBaoService _service;

        public ThongBaoController(IThongBaoService service)
        {
            _service = service;
        }

        // GET: api/ThongBao/user/{taiKhoanId}
        [HttpGet("user/{taiKhoanId}")]
        public IActionResult GetAllByUser(int taiKhoanId)
        {
            try
            {
                var result = _service.GetAllByTaiKhoan(taiKhoanId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/ThongBao/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id, [FromQuery] int taiKhoanId)
        {
            try
            {
                var result = _service.GetById(id, taiKhoanId);

                if (result == null)
                    return NotFound(new { success = false, message = "Không tìm thấy thông báo" });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/ThongBao/unread/{taiKhoanId}
        [HttpGet("unread/{taiKhoanId}")]
        public IActionResult GetUnread(int taiKhoanId)
        {
            try
            {
                var result = _service.GetUnread(taiKhoanId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/ThongBao/loai/{taiKhoanId}/{loai}
        [HttpGet("loai/{taiKhoanId}/{loai}")]
        public IActionResult GetByLoai(int taiKhoanId, string loai)
        {
            try
            {
                var result = _service.GetByLoai(taiKhoanId, loai);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/ThongBao/paged
        [HttpPost("paged")]
        public IActionResult GetPaged([FromBody] ThongBaoFilterDto filter)
        {
            try
            {
                var result = _service.GetPaged(filter);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/ThongBao/thongke/{taiKhoanId}
        [HttpGet("thongke/{taiKhoanId}")]
        public IActionResult GetThongKe(int taiKhoanId)
        {
            try
            {
                var result = _service.GetThongKe(taiKhoanId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/ThongBao
        [HttpPost]
        public IActionResult Create([FromBody] CreateThongBaoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });

                var result = _service.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id, taiKhoanId = result.TaiKhoanId },
                    new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT: api/ThongBao/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateThongBaoDto dto, [FromQuery] int taiKhoanId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });

                var result = _service.Update(id, dto, taiKhoanId);

                if (result == null)
                    return NotFound(new { success = false, message = "Không tìm thấy thông báo" });

                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT: api/ThongBao/mark-read/{id}
        [HttpPut("mark-read/{id}")]
        public IActionResult MarkAsRead(int id, [FromQuery] int taiKhoanId)
        {
            try
            {
                var result = _service.MarkAsRead(id, taiKhoanId);

                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy thông báo" });

                return Ok(new { success = true, message = "Đã đánh dấu đã xem" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT: api/ThongBao/mark-all-read/{taiKhoanId}
        [HttpPut("mark-all-read/{taiKhoanId}")]
        public IActionResult MarkAllAsRead(int taiKhoanId)
        {
            try
            {
                _service.MarkAllAsRead(taiKhoanId);
                return Ok(new { success = true, message = "Đã đánh dấu tất cả đã xem" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/ThongBao/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id, [FromQuery] int taiKhoanId)
        {
            try
            {
                var result = _service.Delete(id, taiKhoanId);

                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy thông báo" });

                return Ok(new { success = true, message = "Đã xóa thông báo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/ThongBao/cleanup/{taiKhoanId}
        [HttpDelete("cleanup/{taiKhoanId}")]
        public IActionResult CleanupOld(int taiKhoanId, [FromQuery] int daysOld = 30)
        {
            try
            {
                var count = _service.CleanupOldNotifications(taiKhoanId, daysOld);
                return Ok(new { success = true, message = $"Đã xóa {count} thông báo cũ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}