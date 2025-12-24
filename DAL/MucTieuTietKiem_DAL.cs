using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Models;
using System.Data;


namespace DAL
{
    public class MucTieuTietKiem_DAL
    {
        private readonly string _connStr;

        public MucTieuTietKiem_DAL(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection")
                      ?? throw new Exception("Missing DefaultConnection");
        }

        public List<MucTieuTietKiem> GetAll(int taiKhoanId, string? trangThai, bool includeDeleted)
        {
            var list = new List<MucTieuTietKiem>();

            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var sql = @"
SELECT Id, TaiKhoanId, TenMucTieu, SoTienCanDat, SoTienDaDat, MoTa, HanHoanThanh, TrangThai, DaXoa, NgayTao, NgayCapNhat
FROM dbo.MucTieuTietKiem
WHERE TaiKhoanId = @TaiKhoanId
  AND (@TrangThai IS NULL OR TrangThai = @TrangThai)
  AND (@IncludeDeleted = 1 OR DaXoa = 0)
ORDER BY NgayCapNhat DESC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@TrangThai", (object?)trangThai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IncludeDeleted", includeDeleted ? 1 : 0);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(Map(rd));
            }
            return list;
        }

        public MucTieuTietKiem? GetById(int id, int taiKhoanId)
        {
            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var sql = @"
SELECT TOP 1 Id, TaiKhoanId, TenMucTieu, SoTienCanDat, SoTienDaDat, MoTa, HanHoanThanh, TrangThai, DaXoa, NgayTao, NgayCapNhat
FROM dbo.MucTieuTietKiem
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            using var rd = cmd.ExecuteReader();
            return rd.Read() ? Map(rd) : null;
        }

        public int Create(MucTieuTietKiemCreateDto dto)
        {
            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var sql = @"
INSERT INTO dbo.MucTieuTietKiem(TaiKhoanId, TenMucTieu, SoTienCanDat, SoTienDaDat, MoTa, HanHoanThanh, TrangThai, DaXoa, NgayTao, NgayCapNhat)
OUTPUT INSERTED.Id
VALUES(@TaiKhoanId, @TenMucTieu, @SoTienCanDat, 0, @MoTa, @HanHoanThanh, N'Đang thực hiện', 0, SYSDATETIME(), SYSDATETIME());";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TaiKhoanId", dto.TaiKhoanId);
            cmd.Parameters.AddWithValue("@TenMucTieu", dto.TenMucTieu);
            cmd.Parameters.AddWithValue("@SoTienCanDat", dto.SoTienCanDat);
            cmd.Parameters.AddWithValue("@MoTa", (object?)dto.MoTa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HanHoanThanh", (object?)dto.HanHoanThanh ?? DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, int taiKhoanId, MucTieuTietKiemUpdateDto dto)
        {
            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var sql = @"
UPDATE dbo.MucTieuTietKiem
SET TenMucTieu = @TenMucTieu,
    SoTienCanDat = @SoTienCanDat,
    MoTa = @MoTa,
    HanHoanThanh = @HanHoanThanh,
    TrangThai = @TrangThai,
    NgayCapNhat = SYSDATETIME()
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@TenMucTieu", dto.TenMucTieu);
            cmd.Parameters.AddWithValue("@SoTienCanDat", dto.SoTienCanDat);
            cmd.Parameters.AddWithValue("@MoTa", (object?)dto.MoTa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HanHoanThanh", (object?)dto.HanHoanThanh ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TrangThai", dto.TrangThai);

            return cmd.ExecuteNonQuery() > 0;
        }

        // Nạp tiền vào mục tiêu (tăng SoTienDaDat)
        public bool AddMoney(int id, int taiKhoanId, decimal soTienNap)
        {
            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var sql = @"
UPDATE dbo.MucTieuTietKiem
SET SoTienDaDat = SoTienDaDat + @SoTienNap,
    NgayCapNhat = SYSDATETIME()
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId AND DaXoa = 0;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@SoTienNap", soTienNap);

            return cmd.ExecuteNonQuery() > 0;
        }

        // Xóa mềm
        public bool SoftDelete(int id, int taiKhoanId)
        {
            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var sql = @"
UPDATE dbo.MucTieuTietKiem
SET DaXoa = 1,
    NgayCapNhat = SYSDATETIME()
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            return cmd.ExecuteNonQuery() > 0;
        }

        // Khôi phục (nếu cần)
        public bool Restore(int id, int taiKhoanId)
        {
            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var sql = @"
UPDATE dbo.MucTieuTietKiem
SET DaXoa = 0,
    NgayCapNhat = SYSDATETIME()
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            return cmd.ExecuteNonQuery() > 0;
        }

        private static MucTieuTietKiem Map(SqlDataReader rd)
        {
            return new MucTieuTietKiem
            {
                Id = (int)rd["Id"],
                TaiKhoanId = (int)rd["TaiKhoanId"],
                TenMucTieu = (string)rd["TenMucTieu"],
                SoTienCanDat = (decimal)rd["SoTienCanDat"],
                SoTienDaDat = (decimal)rd["SoTienDaDat"],
                MoTa = rd["MoTa"] == DBNull.Value ? null : (string)rd["MoTa"],
                HanHoanThanh = rd["HanHoanThanh"] == DBNull.Value ? null : (DateTime)rd["HanHoanThanh"],
                TrangThai = (string)rd["TrangThai"],
                DaXoa = (bool)rd["DaXoa"],
                NgayTao = (DateTime)rd["NgayTao"],
                NgayCapNhat = (DateTime)rd["NgayCapNhat"]
            };
        }
    }
}
