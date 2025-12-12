using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_WalletBudget.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly IGiaoDich_BLL _bll;
        public TransactionsController(IGiaoDich_BLL bll) => _bll = bll;

        // GET /api/transactions?from=&to=&viId=&danhMucId=&loai=&q=&page=&pageSize=&sort=NgayGD_desc
        [HttpGet]
        public IActionResult GetList(
            [FromQuery] int taiKhoanId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? viId,
            [FromQuery] int? danhMucId,
            [FromQuery] string? loai,
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sort = "NgayGD_desc",
            [FromQuery] bool includeDeleted = false)
        {
            try
            {
                var data = _bll.Query(taiKhoanId, from, to, viId, danhMucId, loai, q, page, pageSize, sort, includeDeleted);
                return Ok(data);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET /api/transactions/{id}?taiKhoanId=2
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id, [FromQuery] int taiKhoanId, [FromQuery] bool includeDeleted = false)
        {
            try
            {
                var gd = _bll.GetById(id, taiKhoanId, includeDeleted);
                return gd == null ? NotFound() : Ok(gd);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // POST /api/transactions
        [HttpPost]
        public IActionResult Create([FromBody] TransactionCreateRequest req)
        {
            try
            {
                var id = _bll.Create(req);
                return CreatedAtAction(nameof(GetById), new { id, taiKhoanId = req.TaiKhoanId }, new { id });
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message switch
                {
                    "WALLET_NOT_BELONG_USER" => BadRequest(new { message = "ViId không thuộc user hoặc đã bị xóa." }),
                    "CATEGORY_NOT_BELONG_USER" => BadRequest(new { message = "DanhMucId không thuộc user hoặc đã bị xóa." }),
                    "LOAIGD_NOT_MATCH_CATEGORY" => BadRequest(new { message = "LoaiGD không khớp DanhMuc.Loai." }),
                    _ => BadRequest(new { message = ex.Message })
                };
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT /api/transactions/{id}
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] TransactionUpdateRequest req)
        {
            try
            {
                var ok = _bll.Update(id, req);
                return ok ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message switch
                {
                    "WALLET_NOT_BELONG_USER" => BadRequest(new { message = "ViId không thuộc user hoặc đã bị xóa." }),
                    "CATEGORY_NOT_BELONG_USER" => BadRequest(new { message = "DanhMucId không thuộc user hoặc đã bị xóa." }),
                    "LOAIGD_NOT_MATCH_CATEGORY" => BadRequest(new { message = "LoaiGD không khớp DanhMuc.Loai." }),
                    _ => BadRequest(new { message = ex.Message })
                };
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // DELETE /api/transactions/{id}?taiKhoanId=2
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id, [FromQuery] int taiKhoanId)
        {
            var ok = _bll.Delete(id, taiKhoanId);
            return ok ? NoContent() : NotFound();
        }

        // PATCH /api/transactions/{id}/restore?taiKhoanId=2
        [HttpPatch("{id:int}/restore")]
        public IActionResult Restore(int id, [FromQuery] int taiKhoanId)
        {
            var ok = _bll.Restore(id, taiKhoanId);
            return ok ? NoContent() : NotFound();
        }
    }
}
