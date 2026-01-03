using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Models;

namespace DAL
{
    public class DongGopMucTieu_DAL
    {
        private readonly string _cs;

        public DongGopMucTieu_DAL(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Missing DefaultConnection in appsettings.json");
        }

        public async Task<List<DongGopMucTieu>> GetByMucTieuAsync(int mucTieuId, bool includeDeleted)
        {
            var list = new List<DongGopMucTieu>();
            var sql = @"
                SELECT Id, MucTieuId, TaiKhoanId, GiaoDichId, SoTien, NgayDongGop, GhiChu, DaXoa, NgayTao
                FROM dbo.DongGopMucTieu
                WHERE MucTieuId = @MucTieuId
                  AND (@IncludeDeleted = 1 OR DaXoa = 0)
                ORDER BY NgayDongGop DESC, Id DESC;";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MucTieuId", mucTieuId);
            cmd.Parameters.AddWithValue("@IncludeDeleted", includeDeleted ? 1 : 0);

            await conn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new DongGopMucTieu
                {
                    Id = rd.GetInt32(0),
                    MucTieuId = rd.GetInt32(1),
                    TaiKhoanId = rd.GetInt32(2),
                    // SỬA LỖI: Kiểm tra NULL trước khi đọc GetInt32
                    GiaoDichId = rd.IsDBNull(3) ? (int?)null : rd.GetInt32(3),
                    SoTien = rd.GetDecimal(4),
                    NgayDongGop = rd.GetDateTime(5),
                    GhiChu = rd.IsDBNull(6) ? null : rd.GetString(6),
                    DaXoa = rd.GetBoolean(7),
                    NgayTao = rd.GetDateTime(8)
                });
            }
            return list;
        }

        public async Task<DongGopMucTieu?> GetByIdAsync(int id, int taiKhoanId, bool includeDeleted)
        {
            var sql = @"SELECT Id, MucTieuId, TaiKhoanId, GiaoDichId, SoTien, NgayDongGop, GhiChu, DaXoa, NgayTao
                        FROM dbo.DongGopMucTieu WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId
                        AND (@IncludeDeleted = 1 OR DaXoa = 0)";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@IncludeDeleted", includeDeleted ? 1 : 0);

            await conn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                return new DongGopMucTieu
                {
                    Id = rd.GetInt32(0),
                    MucTieuId = rd.GetInt32(1),
                    TaiKhoanId = rd.GetInt32(2),
                    GiaoDichId = rd.IsDBNull(3) ? null : rd.GetInt32(3), // Tránh lỗi lần 2
                    SoTien = rd.GetDecimal(4),
                    NgayDongGop = rd.GetDateTime(5),
                    GhiChu = rd.IsDBNull(6) ? null : rd.GetString(6),
                    DaXoa = rd.GetBoolean(7),
                    NgayTao = rd.GetDateTime(8)
                };
            }
            return null;
        }

        public async Task<int> CreateAsync(DongGopMucTieuCreateRequest req)
        {
            var sql = @"
            BEGIN TRANSACTION;
            BEGIN TRY

                DECLARE @ViId INT = (SELECT TOP 1 Id FROM dbo.Vi WHERE TaiKhoanId = @TaiKhoanId AND DaXoa = 0);
                DECLARE @DmId INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId = @TaiKhoanId AND TenDanhMuc LIKE N'%Tiết kiệm%');
                IF @DmId IS NULL SET @DmId = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId = @TaiKhoanId);

                INSERT INTO dbo.GiaoDich (TaiKhoanId, ViId, DanhMucId, SoTien, LoaiGD, NgayGD, GhiChu)
                VALUES (@TaiKhoanId, @ViId, @DmId, @SoTien, N'Chi', @NgayDongGop, @GhiChu);
                
                DECLARE @NewGiaoDichId INT = SCOPE_IDENTITY();

                INSERT INTO dbo.DongGopMucTieu (MucTieuId, TaiKhoanId, GiaoDichId, SoTien, NgayDongGop, GhiChu)
                VALUES (@MucTieuId, @TaiKhoanId, @NewGiaoDichId, @SoTien, @NgayDongGop, @GhiChu);
                
                DECLARE @NewId INT = SCOPE_IDENTITY();

                UPDATE dbo.MucTieuTietKiem 
                SET SoTienDaDat = SoTienDaDat + @SoTien 
                WHERE Id = @MucTieuId;

                COMMIT TRANSACTION;
                SELECT @NewId;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MucTieuId", req.MucTieuId);
            cmd.Parameters.AddWithValue("@TaiKhoanId", req.TaiKhoanId);
            cmd.Parameters.AddWithValue("@SoTien", req.SoTien);
            cmd.Parameters.AddWithValue("@NgayDongGop", req.NgayDongGop);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)req.GhiChu ?? DBNull.Value);

            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }


        public async Task<bool> UpdateAsync(int id, int taiKhoanId, DongGopMucTieuUpdateRequest req)
        {
            var sql = @"
UPDATE dbo.DongGopMucTieu
SET SoTien = @SoTien,
    NgayDongGop = @NgayDongGop,
    GhiChu = @GhiChu
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId AND DaXoa = 0;";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@SoTien", req.SoTien);
            cmd.Parameters.AddWithValue("@NgayDongGop", req.NgayDongGop.Date);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)req.GhiChu ?? DBNull.Value);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> SoftDeleteAsync(int id, int taiKhoanId)
        {
            var sql = @"
UPDATE dbo.DongGopMucTieu
SET DaXoa = 1
WHERE Id = @Id AND TaiKhoanId = @TaiKhoanId AND DaXoa = 0;";

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
