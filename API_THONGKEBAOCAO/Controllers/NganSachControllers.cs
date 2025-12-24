using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_THONGKEBAOCAO.Controllers;

[ApiController]
[Route("api/ngansach")]
public class NganSachController : ControllerBase
{
    private readonly INganSach_BLL _bll;
    public NganSachController(INganSach_BLL bll) => _bll = bll;

    // GET /api/ngansach?taiKhoanId=1&includeDeleted=false
    [HttpGet]
    public async Task<IActionResult> GetBudgets(
        [FromQuery] int taiKhoanId,
        [FromQuery] bool includeDeleted = false)
        => Ok(await _bll.GetBudgetsAsync(taiKhoanId, includeDeleted));

    // POST /api/ngansach
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NganSachCreateRequest req)
    {
        try
        {
            var id = await _bll.CreateBudgetAsync(req);
            return CreatedAtAction(nameof(GetByUser),
                new { taiKhoanId = req.TaiKhoanId },
                new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Không thể tạo ngân sách",
                error = ex.Message
            });
        }
    }

    // GET /api/ngansach/user?taiKhoanId=1
    [HttpGet("user")]
    public async Task<IActionResult> GetByUser(
        [FromQuery] int taiKhoanId,
        [FromQuery] bool includeDeleted = false)
        => Ok(await _bll.GetBudgetsAsync(taiKhoanId, includeDeleted));

    // PUT /api/ngansach/{id}?taiKhoanId=1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromQuery] int taiKhoanId,
        [FromBody] NganSachUpdateRequest req)
    {
        try
        {
            var ok = await _bll.UpdateBudgetAsync(id, taiKhoanId, req);
            return ok ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Cập nhật ngân sách thất bại",
                error = ex.Message
            });
        }
    }

    // PATCH /api/ngansach/{id}/lock
    [HttpPatch("{id:int}/lock")]
    public async Task<IActionResult> Lock(
        int id,
        [FromBody] NganSachLockRequest req)
    {
        var ok = await _bll.LockBudgetAsync(id, req.TaiKhoanId, req.IsLocked);
        return ok ? NoContent() : NotFound();
    }

    // DELETE /api/ngansach/{id}?taiKhoanId=1   (xóa mềm)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDelete(
        int id,
        [FromQuery] int taiKhoanId)
    {
        var ok = await _bll.SoftDeleteBudgetAsync(id, taiKhoanId);
        return ok ? NoContent() : NotFound();
    }
}
