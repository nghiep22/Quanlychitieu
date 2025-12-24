using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace API_THONGKEBAOCAO.Controllers;

[ApiController]
[Route("api/donggopmuctieu")]
public class DongGopMucTieuController : ControllerBase
{
    private readonly IDongGopMucTieu_BLL _bll;
    public DongGopMucTieuController(IDongGopMucTieu_BLL bll) => _bll = bll;

    // GET /api/donggopmuctieu/muctieu/3?includeDeleted=false
    [HttpGet("muctieu/{mucTieuId:int}")]
    public async Task<IActionResult> GetByMucTieu(int mucTieuId, [FromQuery] bool includeDeleted = false)
    {
        try
        {
            return Ok(await _bll.GetByMucTieuAsync(mucTieuId, includeDeleted));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/donggopmuctieu/5?taiKhoanId=1&includeDeleted=false
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] int taiKhoanId, [FromQuery] bool includeDeleted = false)
    {
        try
        {
            var item = await _bll.GetByIdAsync(id, taiKhoanId, includeDeleted);
            return item == null ? NotFound() : Ok(item);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // POST /api/donggopmuctieu
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DongGopMucTieuCreateRequest req)
    {
        try
        {
            var id = await _bll.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id, taiKhoanId = req.TaiKhoanId }, new { id });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (SqlException ex) { return BadRequest(new { message = "Lỗi SQL", error = ex.Message }); }
    }

    // PUT /api/donggopmuctieu/5?taiKhoanId=1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromQuery] int taiKhoanId, [FromBody] DongGopMucTieuUpdateRequest req)
    {
        try
        {
            var ok = await _bll.UpdateAsync(id, taiKhoanId, req);
            return ok ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (SqlException ex) { return BadRequest(new { message = "Lỗi SQL", error = ex.Message }); }
    }

    // DELETE /api/donggopmuctieu/5?taiKhoanId=1 (xóa mềm)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDelete(int id, [FromQuery] int taiKhoanId)
    {
        try
        {
            var ok = await _bll.SoftDeleteAsync(id, taiKhoanId);
            return ok ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (SqlException ex) { return BadRequest(new { message = "Lỗi SQL", error = ex.Message }); }
    }
}
