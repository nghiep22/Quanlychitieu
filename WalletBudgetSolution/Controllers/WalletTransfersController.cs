using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_WalletBudget.Controllers
{
    [ApiController]
    [Route("api/wallet-transfers")]
    public class WalletTransfersController : ControllerBase
    {
        private readonly IWalletTransfer_BLL _bll;

        public WalletTransfersController(IWalletTransfer_BLL bll)
        {
            _bll = bll;
        }

        // POST /api/wallet-transfers
        [HttpPost]
        public IActionResult Create([FromBody] WalletTransferCreateRequest req)
        {
            try
            {
                var created = _bll.Create(req);
                return CreatedAtAction(nameof(GetById), new { id = created.Id, taiKhoanId = created.TaiKhoanId }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/wallet-transfers?...
        [HttpGet]
        public IActionResult GetList(
            [FromQuery] int taiKhoanId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? viNguonId,
            [FromQuery] int? viDichId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeDeleted = false)
        {
            var q = new WalletTransferQuery
            {
                TaiKhoanId = taiKhoanId,
                From = from,
                To = to,
                ViNguonId = viNguonId,
                ViDichId = viDichId,
                Page = page,
                PageSize = pageSize,
                IncludeDeleted = includeDeleted
            };

            var list = _bll.GetList(q);
            return Ok(list);
        }

        // GET /api/wallet-transfers/{id}?taiKhoanId=...
        [HttpGet("{id:int}")]
        public IActionResult GetById([FromRoute] int id, [FromQuery] int taiKhoanId)
        {
            var item = _bll.GetById(id, taiKhoanId);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // DELETE /api/wallet-transfers/{id}?taiKhoanId=...
        [HttpDelete("{id:int}")]
        public IActionResult Delete([FromRoute] int id, [FromQuery] int taiKhoanId)
        {
            var ok = _bll.SoftDelete(id, taiKhoanId);
            if (!ok) return NotFound(new { message = "Không tìm thấy transfer hoặc đã xóa." });
            return NoContent(); // 204
        }
    }
}
