using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Models;
using System.Data;

namespace DAL
{
    public class ThongBao_DAL
    {
        private readonly string _cs;

        public ThongBao_DAL(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                  ?? throw new Exception("Missing DefaultConnection in appsettings.json");
        }

        public async Task<List<ThongBao>> GetAllAsync(
            int taiKhoanId,
            string? loai,
            string? trangThai,
            DateTime? from,
            DateTime? to)
        {
            var list = new List<ThongBao>();

            var sql = @"
SELECT Id, TaiKhoanId, TieuDe, NoiDung, Loai, GiaoDichId, NganSachId, MucTieuId, ThoiGianGui, TrangThai
FROM dbo.ThongBao
WHERE TaiKhoanId = @TaiKhoanId
  AND (@Loai IS NULL OR Loai = @Loai)
  AND (@TrangThai IS NULL OR TrangThai = @TrangThai)
  AND (@From IS NULL OR ThoiGianGui >= @From)
  AND (@To IS NULL OR ThoiGianGui <= @To)
ORDER BY ThoiGianGui DESC;";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@Loai", (object?)loai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TrangThai", (object?)trangThai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@From", (object?)from ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@To", (object?)to ?? DBNull.Value);

            await conn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                list.Add(new ThongBao
                {
                    Id = rd.GetInt32(0),
                    TaiKhoanId = rd.GetInt32(1),
                    TieuDe = rd.GetString(2),
                    NoiDung = rd.GetString(3),
                    Loai = rd.GetString(4),
                    GiaoDichId = rd.IsDBNull(5) ? null : rd.GetInt32(5),
                    NganSachId = rd.IsDBNull(6) ? null : rd.GetInt32(6),
                    MucTieuId = rd.IsDBNull(7) ? null : rd.GetInt32(7),
                    ThoiGianGui = rd.GetDateTime(8),
                    TrangThai = rd.GetString(9)
                });
            }

            return list;
        }

        public async Task<ThongBao?> GetByIdAsync(int id, int taiKhoanId)
        {
            var sql = @"
SELECT Id, TaiKhoanId, TieuDe, NoiDung, Loai, GiaoDichId, NganSachId, MucTieuId, ThoiGianGui, TrangThai
FROM dbo.ThongBao
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId;";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            await conn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;

            return new ThongBao
            {
                Id = rd.GetInt32(0),
                TaiKhoanId = rd.GetInt32(1),
                TieuDe = rd.GetString(2),
                NoiDung = rd.GetString(3),
                Loai = rd.GetString(4),
                GiaoDichId = rd.IsDBNull(5) ? null : rd.GetInt32(5),
                NganSachId = rd.IsDBNull(6) ? null : rd.GetInt32(6),
                MucTieuId = rd.IsDBNull(7) ? null : rd.GetInt32(7),
                ThoiGianGui = rd.GetDateTime(8),
                TrangThai = rd.GetString(9)
            };
        }

        public async Task<int> CreateAsync(ThongBaoCreateRequest req)
        {
            var sql = @"
INSERT INTO dbo.ThongBao (TaiKhoanId, TieuDe, NoiDung, Loai, GiaoDichId, NganSachId, MucTieuId)
OUTPUT INSERTED.Id
VALUES (@TaiKhoanId, @TieuDe, @NoiDung, @Loai, @GiaoDichId, @NganSachId, @MucTieuId);";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TaiKhoanId", req.TaiKhoanId);
            cmd.Parameters.AddWithValue("@TieuDe", req.TieuDe);
            cmd.Parameters.AddWithValue("@NoiDung", req.NoiDung);
            cmd.Parameters.AddWithValue("@Loai", req.Loai);
            cmd.Parameters.AddWithValue("@GiaoDichId", (object?)req.GiaoDichId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NganSachId", (object?)req.NganSachId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MucTieuId", (object?)req.MucTieuId ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateTrangThaiAsync(int id, int taiKhoanId, string trangThai)
        {
            var sql = @"
UPDATE dbo.ThongBao
SET TrangThai = @TrangThai
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId;";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@TrangThai", trangThai);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, int taiKhoanId)
        {
            var sql = @"DELETE FROM dbo.ThongBao WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId;";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
