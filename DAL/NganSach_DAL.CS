using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Models;
using System.Data;

namespace DAL;

public class NganSach_DAL
{
    private readonly string _cs;
    public NganSach_DAL(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Missing DefaultConnection");

    private static NganSach Map(SqlDataReader rd) => new()
    {
        Id = (int)rd["Id"],
        TaiKhoanId = (int)rd["TaiKhoanId"],
        DanhMucId = (int)rd["DanhMucId"],
        SoTienGioiHan = (decimal)rd["SoTienGioiHan"],
        TGBatDau = (DateTime)rd["TGBatDau"],
        TGKetThuc = (DateTime)rd["TGKetThuc"],
        TrangThai = (string)rd["TrangThai"],
        GhiChu = rd["GhiChu"] as string,
        DaXoa = (bool)rd["DaXoa"],
        NgayTao = (DateTime)rd["NgayTao"],
        NgayCapNhat = (DateTime)rd["NgayCapNhat"]
    };

    public async Task<List<NganSach>> GetByUserAsync(int taiKhoanId, bool includeDeleted = false)
    {
        var list = new List<NganSach>();
        var sql = @"
SELECT *
FROM dbo.NganSach
WHERE TaiKhoanId=@uid AND (@inc=1 OR DaXoa=0)
ORDER BY TGBatDau DESC;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;
        cmd.Parameters.Add("@inc", SqlDbType.Bit).Value = includeDeleted;

        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) list.Add(Map(rd));
        return list;
    }

    public async Task<int> CreateAsync(NganSachCreateRequest req)
    {
        var sql = @"
INSERT INTO dbo.NganSach
(TaiKhoanId, DanhMucId, SoTienGioiHan, TGBatDau, TGKetThuc, GhiChu)
OUTPUT INSERTED.Id
VALUES (@uid, @dm, @tien, @bd, @kt, @gc);";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);

        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = req.TaiKhoanId;
        cmd.Parameters.Add("@dm", SqlDbType.Int).Value = req.DanhMucId;
        cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = req.SoTienGioiHan;
        cmd.Parameters.Add("@bd", SqlDbType.Date).Value = req.TGBatDau;
        cmd.Parameters.Add("@kt", SqlDbType.Date).Value = req.TGKetThuc;
        cmd.Parameters.Add("@gc", SqlDbType.NVarChar, 500).Value = (object?)req.GhiChu ?? DBNull.Value;

        cmd.Parameters["@tien"].Precision = 18;
        cmd.Parameters["@tien"].Scale = 2;

        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> UpdateAsync(int id, int taiKhoanId, NganSachUpdateRequest req)
    {
        var sql = @"
UPDATE dbo.NganSach
SET DanhMucId=@dm,
    SoTienGioiHan=@tien,
    TGBatDau=@bd,
    TGKetThuc=@kt,
    GhiChu=@gc,
    NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);

        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;
        cmd.Parameters.Add("@dm", SqlDbType.Int).Value = req.DanhMucId;
        cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = req.SoTienGioiHan;
        cmd.Parameters.Add("@bd", SqlDbType.Date).Value = req.TGBatDau;
        cmd.Parameters.Add("@kt", SqlDbType.Date).Value = req.TGKetThuc;
        cmd.Parameters.Add("@gc", SqlDbType.NVarChar, 500).Value = (object?)req.GhiChu ?? DBNull.Value;

        cmd.Parameters["@tien"].Precision = 18;
        cmd.Parameters["@tien"].Scale = 2;

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> SetLockAsync(int id, int taiKhoanId, bool isLocked)
    {
        var sql = @"
UPDATE dbo.NganSach
SET TrangThai=@st, NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);

        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;
        cmd.Parameters.Add("@st", SqlDbType.NVarChar, 50)
            .Value = isLocked ? "Khóa" : "Hoạt động";

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id, int taiKhoanId)
    {
        var sql = @"
UPDATE dbo.NganSach
SET DaXoa=1, NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);

        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@uid", SqlDbType.Int).Value = taiKhoanId;

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public List<NganSach> GetByTaiKhoan(int taiKhoanId)
    {
        var list = new List<NganSach>();

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"
            SELECT ns.*, dm.TenDanhMuc
            FROM NganSach ns
            JOIN DanhMuc dm ON ns.DanhMucId = dm.Id
            WHERE ns.TaiKhoanId = @TaiKhoanId
              AND ns.DaXoa = 0
        ", cn);

        cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
        cn.Open();

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new NganSach
            {
                Id = (int)rd["Id"],
                TaiKhoanId = (int)rd["TaiKhoanId"],
                DanhMucId = (int)rd["DanhMucId"],
                SoTienGioiHan = (decimal)rd["SoTienGioiHan"],
                TGBatDau = (DateTime)rd["TGBatDau"],
                TGKetThuc = (DateTime)rd["TGKetThuc"],
                TrangThai = rd["TrangThai"].ToString()!,
                GhiChu = rd["GhiChu"] as string
            });
        }

        return list;
    }
}
