using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Models;
using System.Data;

namespace DAL;

public class Vi_DAL
{
    private readonly string _cs;
    public Vi_DAL(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")
             ?? throw new Exception("Missing DefaultConnection");

    private static Vi Map(SqlDataReader rd) => new Vi
    {
        Id = (int)rd["Id"],
        TaiKhoanId = (int)rd["TaiKhoanId"],
        TenVi = (string)rd["TenVi"],
        LoaiVi = (string)rd["LoaiVi"],
        SoDuBanDau = (decimal)rd["SoDuBanDau"],
        GhiChu = rd["GhiChu"] as string,
        TrangThai = (string)rd["TrangThai"],
        DaXoa = (bool)rd["DaXoa"],
        NgayTao = (DateTime)rd["NgayTao"],
        NgayCapNhat = (DateTime)rd["NgayCapNhat"]
    };

    public async Task<List<Vi>> GetByUserAsync(int taiKhoanId, bool includeDeleted = false)
    {
        var list = new List<Vi>();
        var sql = @"
SELECT Id, TaiKhoanId, TenVi, LoaiVi, SoDuBanDau, GhiChu, TrangThai, DaXoa, NgayTao, NgayCapNhat
FROM dbo.Vi
WHERE TaiKhoanId=@uid AND (@inc=1 OR DaXoa=0)
ORDER BY Id DESC;";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;
        cmd.Parameters.Add("@inc", SqlDbType.Bit).Value = includeDeleted;

        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) list.Add(Map(rd));
        return list;
    }

    public async Task<Vi?> GetByIdAsync(int id, int taiKhoanId, bool includeDeleted = false)
    {
        var sql = @"
SELECT TOP 1 Id, TaiKhoanId, TenVi, LoaiVi, SoDuBanDau, GhiChu, TrangThai, DaXoa, NgayTao, NgayCapNhat
FROM dbo.Vi
WHERE Id=@id AND TaiKhoanId=@uid AND (@inc=1 OR DaXoa=0);";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;
        cmd.Parameters.Add("@inc", SqlDbType.Bit).Value = includeDeleted;

        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        return await rd.ReadAsync() ? Map(rd) : null;
    }

    public async Task<int> CreateAsync(ViCreateRequest req)
    {
        var sql = @"
INSERT INTO dbo.Vi (TaiKhoanId, TenVi, LoaiVi, SoDuBanDau, GhiChu)
OUTPUT INSERTED.Id
VALUES (@uid, @ten, @loai, @sodu, @ghichu);";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = req.TaiKhoanId;
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = req.TenVi;
        cmd.Parameters.Add("@loai", SqlDbType.NVarChar, 50).Value = req.LoaiVi;
        cmd.Parameters.Add("@sodu", SqlDbType.Decimal).Value = req.SoDuBanDau;
        cmd.Parameters.Add("@ghichu", SqlDbType.NVarChar, 500).Value = (object?)req.GhiChu ?? DBNull.Value;
        cmd.Parameters["@sodu"].Precision = 18;
        cmd.Parameters["@sodu"].Scale = 2;

        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> UpdateAsync(int id, int taiKhoanId, ViUpdateRequest req)
    {
        var sql = @"
UPDATE dbo.Vi
SET TenVi=@ten, LoaiVi=@loai, SoDuBanDau=@sodu, GhiChu=@ghichu,
    NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = req.TenVi;
        cmd.Parameters.Add("@loai", SqlDbType.NVarChar, 50).Value = req.LoaiVi;
        cmd.Parameters.Add("@sodu", SqlDbType.Decimal).Value = req.SoDuBanDau;
        cmd.Parameters.Add("@ghichu", SqlDbType.NVarChar, 500).Value = (object?)req.GhiChu ?? DBNull.Value;
        cmd.Parameters["@sodu"].Precision = 18;
        cmd.Parameters["@sodu"].Scale = 2;

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> SetLockAsync(int id, int taiKhoanId, bool isLocked)
    {
        var sql = @"
UPDATE dbo.Vi
SET TrangThai=@st, NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;
        cmd.Parameters.Add("@st", SqlDbType.NVarChar, 50).Value = isLocked ? "Khóa" : "Hoạt động";

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id, int taiKhoanId)
    {
        var sql = @"
UPDATE dbo.Vi
SET DaXoa=1, NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
