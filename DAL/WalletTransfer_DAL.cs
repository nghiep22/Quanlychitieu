using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Models;
using System.Data;

namespace DAL
{
    public class WalletTransfer_DAL
    {
        private readonly string _conn;

        public WalletTransfer_DAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        // ===== Helpers validate =====
        private int? GetDanhMucId(SqlConnection cn, SqlTransaction tx, int taiKhoanId, string tenDanhMuc, string loai)
        {
            using var cmd = new SqlCommand(@"
SELECT TOP 1 Id
FROM dbo.DanhMuc
WHERE TaiKhoanId=@TaiKhoanId AND TenDanhMuc=@Ten AND Loai=@Loai AND DaXoa=0
", cn, tx);

            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@Ten", tenDanhMuc);
            cmd.Parameters.AddWithValue("@Loai", loai);

            var obj = cmd.ExecuteScalar();
            if (obj == null || obj == DBNull.Value) return null;
            return Convert.ToInt32(obj);
        }

        private bool WalletBelongsToUser(SqlConnection cn, SqlTransaction tx, int taiKhoanId, int viId)
        {
            using var cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM dbo.Vi
WHERE Id=@ViId AND TaiKhoanId=@TaiKhoanId AND DaXoa=0
", cn, tx);

            cmd.Parameters.AddWithValue("@ViId", viId);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private int InsertGiaoDich(SqlConnection cn, SqlTransaction tx,
            int taiKhoanId, int viId, int danhMucId,
            decimal soTien, string loaiGD, DateTime ngayGD, string? ghiChu)
        {
            using var cmd = new SqlCommand(@"
INSERT INTO dbo.GiaoDich
(TaiKhoanId, ViId, DanhMucId, SoTien, LoaiGD, NgayGD, GhiChu)
OUTPUT INSERTED.Id
VALUES
(@TaiKhoanId, @ViId, @DanhMucId, @SoTien, @LoaiGD, @NgayGD, @GhiChu)
", cn, tx);

            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@ViId", viId);
            cmd.Parameters.AddWithValue("@DanhMucId", danhMucId);
            cmd.Parameters.AddWithValue("@SoTien", soTien);
            cmd.Parameters.AddWithValue("@LoaiGD", loaiGD);
            cmd.Parameters.AddWithValue("@NgayGD", ngayGD.Date);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)ghiChu ?? DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ===== POST: Create transfer =====
        public ChuyenTienVi CreateTransfer(WalletTransferCreateRequest req)
        {
            using var cn = new SqlConnection(_conn);
            cn.Open();

            using var tx = cn.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                // 1) validate
                if (req.SoTien <= 0) throw new InvalidOperationException("SoTien phải > 0.");
                if (req.ViNguonId == req.ViDichId) throw new InvalidOperationException("Ví nguồn và ví đích không được trùng.");

                if (!WalletBelongsToUser(cn, tx, req.TaiKhoanId, req.ViNguonId))
                    throw new InvalidOperationException("Ví nguồn không thuộc user hoặc đã bị xóa.");

                if (!WalletBelongsToUser(cn, tx, req.TaiKhoanId, req.ViDichId))
                    throw new InvalidOperationException("Ví đích không thuộc user hoặc đã bị xóa.");

                // 2) lấy DanhMucId theo tên + loại
                var dmChuyenTienDi = GetDanhMucId(cn, tx, req.TaiKhoanId, "Chuyển tiền đi", "CHI");
                var dmNhanChuyenTien = GetDanhMucId(cn, tx, req.TaiKhoanId, "Nhận chuyển tiền", "THU");

                if (dmChuyenTienDi == null)
                    throw new InvalidOperationException("Không tìm thấy danh mục 'Chuyển tiền đi' (CHI) cho user.");

                if (dmNhanChuyenTien == null)
                    throw new InvalidOperationException("Không tìm thấy danh mục 'Nhận chuyển tiền' (THU) cho user.");

                // 3) tạo 2 giao dịch
                string noteBase = string.IsNullOrWhiteSpace(req.GhiChu) ? "" : $" | {req.GhiChu}";

                int gdChiId = InsertGiaoDich(
                    cn, tx,
                    req.TaiKhoanId, req.ViNguonId, dmChuyenTienDi.Value,
                    req.SoTien, "CHI", req.NgayChuyen,
                    $"Chuyển tiền đi{noteBase}"
                );

                int gdThuId = InsertGiaoDich(
                    cn, tx,
                    req.TaiKhoanId, req.ViDichId, dmNhanChuyenTien.Value,
                    req.SoTien, "THU", req.NgayChuyen,
                    $"Nhận chuyển tiền{noteBase}"
                );

                // 4) lưu ChuyenTienVi
                using var cmd = new SqlCommand(@"
INSERT INTO dbo.ChuyenTienVi
(TaiKhoanId, ViNguonId, ViDichId, SoTien, NgayChuyen, GhiChu, GiaoDichChiId, GiaoDichThuId, TrangThai)
OUTPUT INSERTED.Id, INSERTED.NgayTao, INSERTED.DaXoa
VALUES
(@TaiKhoanId, @ViNguonId, @ViDichId, @SoTien, @NgayChuyen, @GhiChu, @GdChiId, @GdThuId, N'Thành công')
", cn, tx);

                cmd.Parameters.AddWithValue("@TaiKhoanId", req.TaiKhoanId);
                cmd.Parameters.AddWithValue("@ViNguonId", req.ViNguonId);
                cmd.Parameters.AddWithValue("@ViDichId", req.ViDichId);
                cmd.Parameters.AddWithValue("@SoTien", req.SoTien);
                cmd.Parameters.AddWithValue("@NgayChuyen", req.NgayChuyen.Date);
                cmd.Parameters.AddWithValue("@GhiChu", (object?)req.GhiChu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GdChiId", gdChiId);
                cmd.Parameters.AddWithValue("@GdThuId", gdThuId);

                using var rd = cmd.ExecuteReader();
                rd.Read();

                var transfer = new ChuyenTienVi
                {
                    Id = rd.GetInt32(0),
                    TaiKhoanId = req.TaiKhoanId,
                    ViNguonId = req.ViNguonId,
                    ViDichId = req.ViDichId,
                    SoTien = req.SoTien,
                    NgayChuyen = req.NgayChuyen.Date,
                    GhiChu = req.GhiChu,
                    GiaoDichChiId = gdChiId,
                    GiaoDichThuId = gdThuId,
                    TrangThai = "Thành công",
                    NgayTao = rd.GetDateTime(1),
                    DaXoa = rd.GetBoolean(2)
                };

                tx.Commit();
                return transfer;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ===== GET list =====
        public List<ChuyenTienVi> GetList(WalletTransferQuery q)
        {
            var list = new List<ChuyenTienVi>();

            using var cn = new SqlConnection(_conn);
            cn.Open();

            int offset = (Math.Max(1, q.Page) - 1) * Math.Max(1, q.PageSize);

            using var cmd = new SqlCommand(@"
SELECT Id, TaiKhoanId, ViNguonId, ViDichId, SoTien, NgayChuyen, GhiChu, GiaoDichChiId, GiaoDichThuId, TrangThai, DaXoa, NgayTao
FROM dbo.ChuyenTienVi
WHERE TaiKhoanId=@TaiKhoanId
  AND (@IncludeDeleted=1 OR DaXoa=0)
  AND (@From IS NULL OR NgayChuyen >= @From)
  AND (@To IS NULL OR NgayChuyen <= @To)
  AND (@ViNguonId IS NULL OR ViNguonId=@ViNguonId)
  AND (@ViDichId IS NULL OR ViDichId=@ViDichId)
ORDER BY NgayChuyen DESC, Id DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
", cn);

            cmd.Parameters.AddWithValue("@TaiKhoanId", q.TaiKhoanId);
            cmd.Parameters.AddWithValue("@IncludeDeleted", q.IncludeDeleted ? 1 : 0);
            cmd.Parameters.AddWithValue("@From", (object?)q.From?.Date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@To", (object?)q.To?.Date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ViNguonId", (object?)q.ViNguonId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ViDichId", (object?)q.ViDichId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", Math.Max(1, q.PageSize));

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new ChuyenTienVi
                {
                    Id = rd.GetInt32(0),
                    TaiKhoanId = rd.GetInt32(1),
                    ViNguonId = rd.GetInt32(2),
                    ViDichId = rd.GetInt32(3),
                    SoTien = rd.GetDecimal(4),
                    NgayChuyen = rd.GetDateTime(5),
                    GhiChu = rd.IsDBNull(6) ? null : rd.GetString(6),
                    GiaoDichChiId = rd.GetInt32(7),
                    GiaoDichThuId = rd.GetInt32(8),
                    TrangThai = rd.GetString(9),
                    DaXoa = rd.GetBoolean(10),
                    NgayTao = rd.GetDateTime(11)
                });
            }

            return list;
        }

        // ===== GET by id =====
        public ChuyenTienVi? GetById(int id, int taiKhoanId)
        {
            using var cn = new SqlConnection(_conn);
            cn.Open();

            using var cmd = new SqlCommand(@"
SELECT TOP 1 Id, TaiKhoanId, ViNguonId, ViDichId, SoTien, NgayChuyen, GhiChu, GiaoDichChiId, GiaoDichThuId, TrangThai, DaXoa, NgayTao
FROM dbo.ChuyenTienVi
WHERE Id=@Id AND TaiKhoanId=@TaiKhoanId
", cn);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return new ChuyenTienVi
            {
                Id = rd.GetInt32(0),
                TaiKhoanId = rd.GetInt32(1),
                ViNguonId = rd.GetInt32(2),
                ViDichId = rd.GetInt32(3),
                SoTien = rd.GetDecimal(4),
                NgayChuyen = rd.GetDateTime(5),
                GhiChu = rd.IsDBNull(6) ? null : rd.GetString(6),
                GiaoDichChiId = rd.GetInt32(7),
                GiaoDichThuId = rd.GetInt32(8),
                TrangThai = rd.GetString(9),
                DaXoa = rd.GetBoolean(10),
                NgayTao = rd.GetDateTime(11)
            };
        }

        // ===== DELETE (soft) =====
        public bool SoftDelete(int id, int taiKhoanId)
        {
            using var cn = new SqlConnection(_conn);
            cn.Open();

            using var tx = cn.BeginTransaction();

            try
            {
                // lấy 2 giao dịch
                int? gdChi = null;
                int? gdThu = null;

                using (var cmdGet = new SqlCommand(@"
SELECT GiaoDichChiId, GiaoDichThuId
FROM dbo.ChuyenTienVi
WHERE Id=@Id AND TaiKhoanId=@TaiKhoanId AND DaXoa=0
", cn, tx))
                {
                    cmdGet.Parameters.AddWithValue("@Id", id);
                    cmdGet.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);

                    using var rd = cmdGet.ExecuteReader();
                    if (!rd.Read())
                    {
                        tx.Rollback();
                        return false;
                    }
                    gdChi = rd.GetInt32(0);
                    gdThu = rd.GetInt32(1);
                }

                // xóa mềm transfer
                using (var cmd1 = new SqlCommand(@"
UPDATE dbo.ChuyenTienVi
SET DaXoa=1
WHERE Id=@Id AND TaiKhoanId=@TaiKhoanId
", cn, tx))
                {
                    cmd1.Parameters.AddWithValue("@Id", id);
                    cmd1.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
                    cmd1.ExecuteNonQuery();
                }

                // xóa mềm 2 giao dịch
                using (var cmd2 = new SqlCommand(@"
UPDATE dbo.GiaoDich
SET DaXoa=1
WHERE TaiKhoanId=@TaiKhoanId AND Id IN (@GdChi, @GdThu)
", cn, tx))
                {
                    cmd2.Parameters.AddWithValue("@TaiKhoanId", taiKhoanId);
                    cmd2.Parameters.AddWithValue("@GdChi", gdChi!.Value);
                    cmd2.Parameters.AddWithValue("@GdThu", gdThu!.Value);
                    cmd2.ExecuteNonQuery();
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
