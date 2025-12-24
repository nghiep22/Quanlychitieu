using BLL;
using Microsoft.AspNetCore.Mvc;

namespace API_WalletBudget.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReports_BLL _bll;

        public ReportsController(IReports_BLL bll)
        {
            _bll = bll;
        }

        // GET /api/reports/summary?month=12&year=2024&taiKhoanId=2&viId=1
        [HttpGet("summary")]
        public IActionResult Summary(
            [FromQuery] int taiKhoanId,
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] int? viId)
        {
            try
            {
                var dto = _bll.Summary(taiKhoanId, month, year, viId);
                return Ok(dto);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET /api/reports/by-category?month=12&year=2024&taiKhoanId=2&viId=1
        [HttpGet("by-category")]
        public IActionResult ByCategory(
            [FromQuery] int taiKhoanId,
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] int? viId)
        {
            try
            {
                var list = _bll.ByCategory(taiKhoanId, month, year, viId);
                return Ok(list);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET /api/reports/by-wallet?month=12&year=2024&taiKhoanId=2
        [HttpGet("by-wallet")]
        public IActionResult ByWallet(
            [FromQuery] int taiKhoanId,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            try
            {
                var list = _bll.ByWallet(taiKhoanId, month, year);
                return Ok(list);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET /api/reports/cashflow?from=2024-12-01&to=2024-12-31&groupBy=day&taiKhoanId=2&viId=1
        [HttpGet("cashflow")]
        public IActionResult Cashflow(
            [FromQuery] int taiKhoanId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string groupBy = "day",
            [FromQuery] int? viId = null)
        {
            try
            {
                var list = _bll.Cashflow(taiKhoanId, from, to, groupBy, viId);
                return Ok(list);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
