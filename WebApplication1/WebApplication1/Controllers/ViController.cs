using Microsoft.AspNetCore.Mvc;
using QL_NganQuy.DTO;
using QL_NganQuy.Services;

namespace QL_NganQuy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViController : ControllerBase
    {
        private readonly IViService _service;

        public ViController(IViService service)
        {
            _service = service;
        }

        // GET: api/Vi
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

        // GET: api/Vi/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var result = _service.GetById(id);
                if (result == null)
                    return NotFound(new { success = false, message = "Không tìm thấy ví" });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/Vi/user/{taiKhoanId}
        [HttpGet("user/{taiKhoanId}")]
        public IActionResult GetByTaiKhoanId(int taiKhoanId)
        {
            try
            {
                var result = _service.GetByTaiKhoanId(taiKhoanId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/Vi/search
        [HttpPost("search")]
        public IActionResult Search([FromBody] ViFilterDto filter)
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

        // POST: api/Vi
        [HttpPost]
        public IActionResult Create([FromBody] CreateViDto dto)
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

        // PUT: api/Vi/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateViDto dto)
        {
            try
            {
                var result = _service.Update(id, dto);
                if (result == null)
                    return NotFound(new { success = false, message = "Không tìm thấy ví" });

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

        // DELETE: api/Vi/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var result = _service.Delete(id);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy ví" });

                return Ok(new { success = true, message = "Xóa ví thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}