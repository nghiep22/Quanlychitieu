using Microsoft.AspNetCore.Mvc;
using QL_NganQuy.DTO;
using QL_NganQuy.Services;

namespace QL_NganQuy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        private readonly ITaiKhoanService _service;

        public TaiKhoanController(ITaiKhoanService service)
        {
            _service = service;
        }

        // GET: api/TaiKhoan
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var result = _service.GetAll();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/TaiKhoan/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var result = _service.GetById(id);
                if (result == null)
                    return NotFound(new { success = false, message = "Không tìm thấy tài khoản" });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/TaiKhoan/search
        [HttpPost("search")]
        public IActionResult Search([FromBody] TaiKhoanFilterDto filter)
        {
            try
            {
                var result = _service.Search(filter);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/TaiKhoan
        [HttpPost]
        public IActionResult Create([FromBody] CreateTaiKhoanDto dto)
        {
            try
            {
                var result = _service.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
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

        // PUT: api/TaiKhoan/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateTaiKhoanDto dto)
        {
            try
            {
                var result = _service.Update(id, dto);
                if (result == null)
                    return NotFound(new { success = false, message = "Không tìm thấy tài khoản" });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/TaiKhoan/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var result = _service.Delete(id);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy tài khoản" });

                return Ok(new { success = true, message = "Xóa tài khoản thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}