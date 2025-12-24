using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace API_THONGKEBAOCAO.Controllers;

[ApiController]
[Route("api/thongbao")]
public class ThongBaoController : ControllerBase
{
    private readonly IThongBao_BLL _bll;
    public ThongBaoController(IThongBao_BLL bll) => _bll = bll;

    // GET /api/thongbao?taiKhoanId=1&loai=Thông tin&trangThai=Chưa xem&from=2025-01-01&to=2025-12-31
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int taiKhoanId,
        [FromQuery] string? loai,
        [FromQuery] string? trangThai,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
        {
            return Ok(await _bll.GetAllAsync(taiKhoanId, loai, trangThai, from, to));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/thongbao/{id}?taiKhoanId=1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] int taiKhoanId)
    {
        try
        {
            var item = await _bll.GetByIdAsync(id, taiKhoanId);
            return item == null ? NotFound() : Ok(item);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // POST /api/thongbao
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ThongBaoCreateRequest req)
    {
        try
        {
            var id = await _bll.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id, taiKhoanId = req.TaiKhoanId }, new { id });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (SqlException ex) { return BadRequest(new { message = "Lỗi SQL", error = ex.Message }); }
    }

    // PATCH /api/thongbao/{id}/trangthai
    [HttpPatch("{id:int}/trangthai")]
    public async Task<IActionResult> UpdateTrangThai(int id, [FromBody] ThongBaoUpdateTrangThaiRequest req)
    {
        try
        {
            var ok = await _bll.UpdateTrangThaiAsync(id, req.TaiKhoanId, req.DaXem);
            return ok ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // DELETE /api/thongbao/{id}?taiKhoanId=1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int taiKhoanId)
    {
        try
        {
            var ok = await _bll.DeleteAsync(id, taiKhoanId);
            return ok ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
