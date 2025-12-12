using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Models;
using System.Data;

namespace DAL
{
    public class GiaoDich_DAL
    {
        private readonly string _cs;
        public GiaoDich_DAL(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")!
                  ?? throw new Exception("Missing DefaultConnection");
        }

        private static GiaoDich Map(SqlDataReader rd) => new GiaoDich
        {
            Id = (int)rd["Id"],
            TaiKhoanId = (int)rd["TaiKhoanId"],
            ViId = (int)rd["ViId"],
            DanhMucId = (int)rd["DanhMucId"],
            SoTien = (decimal)rd["SoTien"],
            LoaiGD = rd["LoaiGD"].ToString() ?? "",
            NgayGD = (DateTime)rd["NgayGD"],
            GhiChu = rd["GhiChu"] as string,
            AnhHoaDon = rd["AnhHoaDon"] as string,
            DaXoa = (bool)rd["DaXoa"],
            NgayTao = (DateTime)rd["NgayTao"],
            NgayCapNhat = (DateTime)rd["NgayCapNhat"]
        };

        // Kiểm tra ViId thuộc user và chưa xóa
        public bool WalletBelongsToUser(int taiKhoanId, int viId)
        {
            using var cn = new SqlConnection(_cs);
            cn.Open();
            var sql = @"SELECT COUNT(1) FROM dbo.Vi WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", viId);
            cmd.Parameters.AddWithValue("@uid", taiKhoanId);
            return (int)cmd.ExecuteScalar()! > 0;
        }

        // Lấy Loai của DanhMuc và kiểm tra thuộc user
        public string? GetDanhMucLoaiIfBelongsToUser(int taiKhoanId, int danhMucId)
        {
            using var cn = new SqlConnection(_cs);
            cn.Open();
            var sql = @"SELECT TOP 1 Loai FROM dbo.DanhMuc WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", danhMucId);
            cmd.Parameters.AddWithValue("@uid", taiKhoanId);
            return cmd.ExecuteScalar() as string; // null nếu không thuộc
        }

        public GiaoDich? GetById(int id, int taiKhoanId, bool includeDeleted = false)
        {
            using var cn = new SqlConnection(_cs);
            cn.Open();
            var sql = @"
SELECT TOP 1 Id,TaiKhoanId,ViId,DanhMucId,SoTien,LoaiGD,NgayGD,GhiChu,AnhHoaDon,DaXoa,NgayTao,NgayCapNhat
FROM dbo.GiaoDich
WHERE Id=@id AND TaiKhoanId=@uid
  AND (@inc=1 OR DaXoa=0);";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@uid", taiKhoanId);
            cmd.Parameters.AddWithValue("@inc", includeDeleted ? 1 : 0);

            using var rd = cmd.ExecuteReader();
            return rd.Read() ? Map(rd) : null;
        }

        // LIST: filter + q + paging + sort
        public List<GiaoDich> Query(
            int taiKhoanId,
            DateTime? from, DateTime? to,
            int? viId, int? danhMucId,
            string? loai,
            string? q,
            int page, int pageSize,
            string sort,
            bool includeDeleted = false)
        {
            // sort cho phép: NgayGD_desc / NgayGD_asc / Id_desc / Id_asc
            var orderBy = sort switch
            {
                "NgayGD_asc" => "NgayGD ASC, Id ASC",
                "Id_asc" => "Id ASC",
                "Id_desc" => "Id DESC",
                _ => "NgayGD DESC, Id DESC"
            };

            using var cn = new SqlConnection(_cs);
            cn.Open();

            var sql = $@"
SELECT Id,TaiKhoanId,ViId,DanhMucId,SoTien,LoaiGD,NgayGD,GhiChu,AnhHoaDon,DaXoa,NgayTao,NgayCapNhat
FROM dbo.GiaoDich
WHERE TaiKhoanId=@uid
  AND (@inc=1 OR DaXoa=0)
  AND (@from IS NULL OR NgayGD >= @from)
  AND (@to IS NULL OR NgayGD <= @to)
  AND (@viId IS NULL OR ViId = @viId)
  AND (@dmId IS NULL OR DanhMucId = @dmId)
  AND (@loai IS NULL OR LoaiGD = @loai)
  AND (@q IS NULL OR (GhiChu LIKE N'%' + @q + N'%' OR AnhHoaDon LIKE N'%' + @q + N'%'))
ORDER BY {orderBy}
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@uid", taiKhoanId);
            cmd.Parameters.AddWithValue("@inc", includeDeleted ? 1 : 0);
            cmd.Parameters.AddWithValue("@from", (object?)from ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@to", (object?)to ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@viId", (object?)viId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dmId", (object?)danhMucId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@loai", (object?)loai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@q", string.IsNullOrWhiteSpace(q) ? DBNull.Value : q!.Trim());

            var skip = (page <= 1 ? 0 : (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", pageSize);

            var list = new List<GiaoDich>();
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(Map(rd));
            return list;
        }

        public int Insert(TransactionCreateRequest req)
        {
            using var cn = new SqlConnection(_cs);
            cn.Open();

            var sql = @"
INSERT INTO dbo.GiaoDich (TaiKhoanId,ViId,DanhMucId,SoTien,LoaiGD,NgayGD,GhiChu,AnhHoaDon,DaXoa)
OUTPUT INSERTED.Id
VALUES (@uid,@vi,@dm,@money,@loai,@ngay,@note,@img,0);";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@uid", req.TaiKhoanId);
            cmd.Parameters.AddWithValue("@vi", req.ViId);
            cmd.Parameters.AddWithValue("@dm", req.DanhMucId);

            var p = cmd.Parameters.Add("@money", SqlDbType.Decimal);
            p.Precision = 18; p.Scale = 2; p.Value = req.SoTien;

            cmd.Parameters.AddWithValue("@loai", req.LoaiGD);
            cmd.Parameters.AddWithValue("@ngay", req.NgayGD.Date);
            cmd.Parameters.AddWithValue("@note", (object?)req.GhiChu ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@img", (object?)req.AnhHoaDon ?? DBNull.Value);

            return (int)cmd.ExecuteScalar()!;
        }

        public bool Update(int id, TransactionUpdateRequest req)
        {
            using var cn = new SqlConnection(_cs);
            cn.Open();

            var sql = @"
UPDATE dbo.GiaoDich
SET ViId=@vi, DanhMucId=@dm, SoTien=@money, LoaiGD=@loai,
    NgayGD=@ngay, GhiChu=@note, AnhHoaDon=@img,
    NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@uid", req.TaiKhoanId);
            cmd.Parameters.AddWithValue("@vi", req.ViId);
            cmd.Parameters.AddWithValue("@dm", req.DanhMucId);

            var p = cmd.Parameters.Add("@money", SqlDbType.Decimal);
            p.Precision = 18; p.Scale = 2; p.Value = req.SoTien;

            cmd.Parameters.AddWithValue("@loai", req.LoaiGD);
            cmd.Parameters.AddWithValue("@ngay", req.NgayGD.Date);
            cmd.Parameters.AddWithValue("@note", (object?)req.GhiChu ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@img", (object?)req.AnhHoaDon ?? DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool SoftDelete(int id, int taiKhoanId)
        {
            using var cn = new SqlConnection(_cs);
            cn.Open();

            var sql = @"
UPDATE dbo.GiaoDich
SET DaXoa=1, NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=0;";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@uid", taiKhoanId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Restore(int id, int taiKhoanId)
        {
            using var cn = new SqlConnection(_cs);
            cn.Open();

            var sql = @"
UPDATE dbo.GiaoDich
SET DaXoa=0, NgayCapNhat=SYSDATETIME()
WHERE Id=@id AND TaiKhoanId=@uid AND DaXoa=1;";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@uid", taiKhoanId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
