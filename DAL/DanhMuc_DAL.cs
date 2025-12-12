using Microsoft.Extensions.Configuration;
using Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class DanhMuc_DAL
    {
        private readonly string _connStr;

        public DanhMuc_DAL(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection GetConn() => new SqlConnection(_connStr);

        public List<DanhMuc> GetAll(int taiKhoanId, string? loai, string? status, bool includeDeleted)
        {
            var list = new List<DanhMuc>();

            using var conn = GetConn();
            conn.Open();

            var sql = @"
SELECT Id, TaiKhoanId, TenDanhMuc, Loai, Icon, MauSac, GhiChu, TrangThai, DaXoa, NgayTao, NgayCapNhat
FROM dbo.DanhMuc
WHERE TaiKhoanId = @TaiKhoanId
  AND (@Loai IS NULL OR Loai = @Loai)
  AND (@Status IS NULL OR TrangThai = @Status)
  AND (@IncludeDeleted = 1 OR DaXoa = 0)
ORDER BY Loai, TenDanhMuc;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@Loai", (object?)loai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IncludeDeleted", includeDeleted ? 1 : 0);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(Map(rd));
            }

            return list;
        }

        public DanhMuc? GetById(int id, int taiKhoanId, bool includeDeleted)
        {
            using var conn = GetConn();
            conn.Open();

            var sql = @"
SELECT TOP 1 Id, TaiKhoanId, TenDanhMuc, Loai, Icon, MauSac, GhiChu, TrangThai, DaXoa, NgayTao, NgayCapNhat
FROM dbo.DanhMuc
WHERE Id=@Id AND TaiKhoanId=@TaiKhoanId
  AND (@IncludeDeleted = 1 OR DaXoa = 0);
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@IncludeDeleted", includeDeleted ? 1 : 0);

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return Map(rd);
        }

        public bool ExistsTenLoai(int taiKhoanId, string tenDanhMuc, string loai, int? excludeId = null)
        {
            using var conn = GetConn();
            conn.Open();

            var sql = @"
SELECT COUNT(1)
FROM dbo.DanhMuc
WHERE TaiKhoanId=@TaiKhoanId
  AND DaXoa=0
  AND TenDanhMuc=@TenDanhMuc
  AND Loai=@Loai
  AND (@ExcludeId IS NULL OR Id <> @ExcludeId);
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@TenDanhMuc", tenDanhMuc);
            cmd.Parameters.AddWithValue("@Loai", loai);
            cmd.Parameters.AddWithValue("@ExcludeId", (object?)excludeId ?? DBNull.Value);

            var count = (int)cmd.ExecuteScalar()!;
            return count > 0;
        }

        public int Insert(DanhMuc dm)
        {
            using var conn = GetConn();
            conn.Open();

            var sql = @"
INSERT INTO dbo.DanhMuc (TaiKhoanId, TenDanhMuc, Loai, Icon, MauSac, GhiChu, TrangThai, DaXoa)
OUTPUT INSERTED.Id
VALUES (@TaiKhoanId, @TenDanhMuc, @Loai, @Icon, @MauSac, @GhiChu, N'Hoạt động', 0);
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TaiKhoanId", dm.TaiKhoanId);
            cmd.Parameters.AddWithValue("@TenDanhMuc", dm.TenDanhMuc);
            cmd.Parameters.AddWithValue("@Loai", dm.Loai);
            cmd.Parameters.AddWithValue("@Icon", (object?)dm.Icon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MauSac", (object?)dm.MauSac ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)dm.GhiChu ?? DBNull.Value);

            return (int)cmd.ExecuteScalar()!;
        }

        public bool Update(int id, DanhMuc dm)
        {
            using var conn = GetConn();
            conn.Open();

            var sql = @"
UPDATE dbo.DanhMuc
SET TenDanhMuc=@TenDanhMuc,
    Loai=@Loai,
    Icon=@Icon,
    MauSac=@MauSac,
    GhiChu=@GhiChu,
    NgayCapNhat=SYSDATETIME()
WHERE Id=@Id AND TaiKhoanId=@TaiKhoanId AND DaXoa=0;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", dm.TaiKhoanId);
            cmd.Parameters.AddWithValue("@TenDanhMuc", dm.TenDanhMuc);
            cmd.Parameters.AddWithValue("@Loai", dm.Loai);
            cmd.Parameters.AddWithValue("@Icon", (object?)dm.Icon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MauSac", (object?)dm.MauSac ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)dm.GhiChu ?? DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool SetLock(int id, int taiKhoanId, bool isLocked)
        {
            using var conn = GetConn();
            conn.Open();

            var sql = @"
UPDATE dbo.DanhMuc
SET TrangThai = CASE WHEN @IsLocked=1 THEN N'Khóa' ELSE N'Hoạt động' END,
    NgayCapNhat = SYSDATETIME()
WHERE Id=@Id AND TaiKhoanId=@TaiKhoanId AND DaXoa=0;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@IsLocked", isLocked ? 1 : 0);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool SoftDelete(int id, int taiKhoanId)
        {
            using var conn = GetConn();
            conn.Open();

            var sql = @"
UPDATE dbo.DanhMuc
SET DaXoa=1, NgayCapNhat=SYSDATETIME()
WHERE Id=@Id AND TaiKhoanId=@TaiKhoanId AND DaXoa=0;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            return cmd.ExecuteNonQuery() > 0;
        }

        private static DanhMuc Map(SqlDataReader rd)
        {
            return new DanhMuc
            {
                Id = rd.GetInt32(0),
                TaiKhoanId = rd.GetInt32(1),
                TenDanhMuc = rd.GetString(2),
                Loai = rd.GetString(3),
                Icon = rd.IsDBNull(4) ? null : rd.GetString(4),
                MauSac = rd.IsDBNull(5) ? null : rd.GetString(5),
                GhiChu = rd.IsDBNull(6) ? null : rd.GetString(6),
                TrangThai = rd.GetString(7),
                DaXoa = rd.GetBoolean(8),
                NgayTao = rd.GetDateTime(9),
                NgayCapNhat = rd.GetDateTime(10)
            };
        }
    }
}
