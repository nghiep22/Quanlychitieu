using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_THONGKEBAOCAO.Controllers
{
    [ApiController]
    [Route("api/muctieutietkiem")]
    public class MucTieuTietKiemController : ControllerBase
    {
        private readonly IMucTieuTietKiem_BLL _bll;

        public MucTieuTietKiemController(IMucTieuTietKiem_BLL bll)
        {
            _bll = bll;
        }

        // GET /api/saving-goals?taiKhoanId=2&trangThai=Đang thực hiện&includeDeleted=false
        [HttpGet]
        public IActionResult GetAll([FromQuery] int taiKhoanId, [FromQuery] string? trangThai, [FromQuery] bool includeDeleted = false)
        {
            try
            {
                var list = _bll.GetAll(taiKhoanId, trangThai, includeDeleted);
                return Ok(list);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET /api/saving-goals/5?taiKhoanId=2
        [HttpGet("{id:int}")]
        public IActionResult GetById([FromRoute] int id, [FromQuery] int taiKhoanId)
        {
            try
            {
                var item = _bll.GetById(id, taiKhoanId);
                return Ok(item);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // POST /api/saving-goals
        [HttpPost]
        public IActionResult Create([FromBody] MucTieuTietKiemCreateDto dto)
        {
            try
            {
                var newId = _bll.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = newId, taiKhoanId = dto.TaiKhoanId }, new { id = newId });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT /api/saving-goals/5?taiKhoanId=2
        [HttpPut("{id:int}")]
        public IActionResult Update([FromRoute] int id, [FromQuery] int taiKhoanId, [FromBody] MucTieuTietKiemUpdateDto dto)
        {
            try
            {
                _bll.Update(id, taiKhoanId, dto);
                return Ok(new { message = "Cập nhật thành công." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // POST /api/saving-goals/5/add-money?taiKhoanId=2
        [HttpPost("{id:int}/add-money")]
        public IActionResult AddMoney([FromRoute] int id, [FromQuery] int taiKhoanId, [FromBody] MucTieuTietKiemNapDto dto)
        {
            try
            {
                _bll.AddMoney(id, taiKhoanId, dto.SoTienNap);
                return Ok(new { message = "Nạp tiền vào mục tiêu thành công." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // DELETE /api/saving-goals/5?taiKhoanId=2  (xóa mềm)
        [HttpDelete("{id:int}")]
        public IActionResult SoftDelete([FromRoute] int id, [FromQuery] int taiKhoanId)
        {
            try
            {
                _bll.SoftDelete(id, taiKhoanId);
                return Ok(new { message = "Đã xóa (mềm)." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // POST /api/saving-goals/5/restore?taiKhoanId=2
        [HttpPost("{id:int}/restore")]
        public IActionResult Restore([FromRoute] int id, [FromQuery] int taiKhoanId)
        {
            try
            {
                _bll.Restore(id, taiKhoanId);
                return Ok(new { message = "Khôi phục thành công." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}
