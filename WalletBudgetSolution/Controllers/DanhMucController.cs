using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_WalletBudget.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly IDanhMuc_BLL _bll;

        public CategoriesController(IDanhMuc_BLL bll)
        {
            _bll = bll;
        }

        // GET /api/categories?taiKhoanId=2&loai=THU&status=Hoạt động&includeDeleted=false
        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int taiKhoanId,
            [FromQuery] string? loai,
            [FromQuery] string? status,
            [FromQuery] bool includeDeleted = false)
        {
            try
            {
                var list = _bll.GetAll(taiKhoanId, loai, status, includeDeleted);
                return Ok(list);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET /api/categories/{id}?taiKhoanId=2
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id, [FromQuery] int taiKhoanId, [FromQuery] bool includeDeleted = false)
        {
            try
            {
                var dm = _bll.GetById(id, taiKhoanId, includeDeleted);
                if (dm == null) return NotFound();
                return Ok(dm);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // POST /api/categories
        [HttpPost]
        public IActionResult Create([FromBody] DanhMucCreateRequest req)
        {
            try
            {
                var newId = _bll.Create(req);
                return CreatedAtAction(nameof(GetById), new { id = newId, taiKhoanId = req.TaiKhoanId }, new { id = newId });
            }
            catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE_NAME_LOAI")
            {
                return Conflict(new { message = "TenDanhMuc đã tồn tại trong cùng TaiKhoanId + Loai (DaXoa=0)." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT /api/categories/{id}
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] DanhMucUpdateRequest req)
        {
            try
            {
                var ok = _bll.Update(id, req);
                if (!ok) return NotFound();
                return NoContent(); // 204
            }
            catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE_NAME_LOAI")
            {
                return Conflict(new { message = "TenDanhMuc đã tồn tại trong cùng TaiKhoanId + Loai (DaXoa=0)." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PATCH /api/categories/{id}/lock
        [HttpPatch("{id:int}/lock")]
        public IActionResult Lock(int id, [FromBody] DanhMucLockRequest req)
        {
            try
            {
                var ok = _bll.Lock(id, req);
                if (!ok) return NotFound();
                return NoContent(); // 204
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // DELETE /api/categories/{id}?taiKhoanId=2  (soft delete)
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id, [FromQuery] int taiKhoanId)
        {
            try
            {
                var ok = _bll.Delete(id, taiKhoanId);
                if (!ok) return NotFound();
                return NoContent(); // 204
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }


    }
}
