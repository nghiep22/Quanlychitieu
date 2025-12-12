using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace API_WalletBudget.Controllers;

[ApiController]
[Route("api/wallets")]
public class WalletsController : ControllerBase
{
    private readonly IVi_BLL _bll;
    public WalletsController(IVi_BLL bll) => _bll = bll;

    // GET /api/wallets?taiKhoanId=1&includeDeleted=false
    [HttpGet]
    public async Task<IActionResult> GetWallets([FromQuery] int taiKhoanId, [FromQuery] bool includeDeleted = false)
        => Ok(await _bll.GetWalletsAsync(taiKhoanId, includeDeleted));

    // POST /api/wallets
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ViCreateRequest req)
    {
        try
        {
            var id = await _bll.CreateWalletAsync(req);
            return CreatedAtAction(nameof(GetOne), new { id, taiKhoanId = req.TaiKhoanId }, new { id });
        }
        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            // trùng TenVi theo unique index UX_Vi_User_TenVi_NotDeleted
            return Conflict(new { message = "Tên ví đã tồn tại (chưa bị xóa)." });
        }
    }

    // GET /api/wallets/{id}?taiKhoanId=1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id, [FromQuery] int taiKhoanId, [FromQuery] bool includeDeleted = false)
    {
        var w = await _bll.GetWalletAsync(id, taiKhoanId, includeDeleted);
        return w == null ? NotFound() : Ok(w);
    }

    // PUT /api/wallets/{id}?taiKhoanId=1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromQuery] int taiKhoanId, [FromBody] ViUpdateRequest req)
    {
        try
        {
            var ok = await _bll.UpdateWalletAsync(id, taiKhoanId, req);
            return ok ? NoContent() : NotFound();
        }
        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return Conflict(new { message = "Tên ví đã tồn tại (chưa bị xóa)." });
        }
    }

    // PATCH /api/wallets/{id}/lock
    [HttpPatch("{id:int}/lock")]
    public async Task<IActionResult> Lock(int id, [FromBody] WalletLockRequest req)
    {
        var ok = await _bll.LockWalletAsync(id, req.TaiKhoanId, req.IsLocked);
        return ok ? NoContent() : NotFound();
    }

    // DELETE /api/wallets/{id}?taiKhoanId=1  (xóa mềm)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDelete(int id, [FromQuery] int taiKhoanId)
    {
        var ok = await _bll.SoftDeleteWalletAsync(id, taiKhoanId);
        return ok ? NoContent() : NotFound();
    }
}
