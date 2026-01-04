using System.Data;
using System.Data.SqlClient;
using QL_NganQuy.Database;
using QL_NganQuy.Models;

namespace QL_NganQuy.Repositories
{
    public class BaoCaoRepository : IBaoCaoRepository
    {
        private readonly DbConnection _dbConnection;

        public BaoCaoRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public (decimal tongThu, decimal tongChi, int soGDThu, int soGDChi)
            GetTongQuanThuChi(int taiKhoanId, DateTime tuNgay, DateTime denNgay, int? viId, int? danhMucId)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            SELECT 
                SUM(CASE WHEN LoaiGD = 'THU' THEN SoTien ELSE 0 END) AS TongThu,
                SUM(CASE WHEN LoaiGD = 'CHI' THEN SoTien ELSE 0 END) AS TongChi,
                COUNT(CASE WHEN LoaiGD = 'THU' THEN 1 END) AS SoGDThu,
                COUNT(CASE WHEN LoaiGD = 'CHI' THEN 1 END) AS SoGDChi
            FROM GiaoDich
            WHERE TaiKhoanId = @TaiKhoanId
              AND NgayGD >= @TuNgay
              AND NgayGD <= @DenNgay
              AND DaXoa = 0
        ";

                if (viId.HasValue)
                {
                    cmd.CommandText += " AND ViId = @ViId";
                    cmd.Parameters.Add(new SqlParameter("@ViId", viId.Value));
                }

                if (danhMucId.HasValue)
                {
                    cmd.CommandText += " AND DanhMucId = @DanhMucId";
                    cmd.Parameters.Add(new SqlParameter("@DanhMucId", danhMucId.Value));
                }

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay));

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (
                            reader["TongThu"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TongThu"]),
                            reader["TongChi"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TongChi"]),
                            reader["SoGDThu"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SoGDThu"]),
                            reader["SoGDChi"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SoGDChi"])
                        );
                    }
                }
            }

            return (0, 0, 0, 0);
        }

        public List<(int danhMucId, string tenDanhMuc, string loai, decimal tongTien, int soGD)> GetBaoCaoTheoDanhMuc(int taiKhoanId, DateTime tuNgay, DateTime denNgay, string loaiGD)
        {
            var list = new List<(int, string, string, decimal, int)>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        dm.Id as DanhMucId,
                        dm.TenDanhMuc,
                        dm.Loai,
                        SUM(gd.SoTien) as TongTien,
                        COUNT(gd.Id) as SoGD
                    FROM GiaoDich gd
                    INNER JOIN DanhMuc dm ON gd.DanhMucId = dm.Id
                    WHERE gd.TaiKhoanId = @TaiKhoanId 
                    AND gd.NgayGD >= @TuNgay 
                    AND gd.NgayGD <= @DenNgay 
                    AND gd.DaXoa = 0
                    " + (string.IsNullOrEmpty(loaiGD) ? "" : "AND gd.LoaiGD = @LoaiGD") + @"
                    GROUP BY dm.Id, dm.TenDanhMuc, dm.Loai
                    ORDER BY TongTien DESC";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay));
                if (!string.IsNullOrEmpty(loaiGD))
                    cmd.Parameters.Add(new SqlParameter("@LoaiGD", loaiGD));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((
                            Convert.ToInt32(reader["DanhMucId"]),
                            reader["TenDanhMuc"].ToString(),
                            reader["Loai"].ToString(),
                            Convert.ToDecimal(reader["TongTien"]),
                            Convert.ToInt32(reader["SoGD"])
                        ));
                    }
                }
            }
            return list;
        }

        public List<(int viId, string tenVi, string loaiVi, decimal soDuBanDau, decimal tongThu, decimal tongChi)> GetBaoCaoTheoVi(int taiKhoanId, DateTime tuNgay, DateTime denNgay)
        {
            var list = new List<(int, string, string, decimal, decimal, decimal)>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        v.Id as ViId,
                        v.TenVi,
                        v.LoaiVi,
                        v.SoDuBanDau,
                        ISNULL(SUM(CASE WHEN gd.LoaiGD = 'THU' THEN gd.SoTien ELSE 0 END), 0) as TongThu,
                        ISNULL(SUM(CASE WHEN gd.LoaiGD = 'CHI' THEN gd.SoTien ELSE 0 END), 0) as TongChi
                    FROM Vi v
                    LEFT JOIN GiaoDich gd ON v.Id = gd.ViId 
                        AND gd.NgayGD >= @TuNgay 
                        AND gd.NgayGD <= @DenNgay 
                        AND gd.DaXoa = 0
                    WHERE v.TaiKhoanId = @TaiKhoanId AND v.DaXoa = 0
                    GROUP BY v.Id, v.TenVi, v.LoaiVi, v.SoDuBanDau";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((
                            Convert.ToInt32(reader["ViId"]),
                            reader["TenVi"].ToString(),
                            reader["LoaiVi"].ToString(),
                            Convert.ToDecimal(reader["SoDuBanDau"]),
                            Convert.ToDecimal(reader["TongThu"]),
                            Convert.ToDecimal(reader["TongChi"])
                        ));
                    }
                }
            }
            return list;
        }

        public List<(DateTime ngay, decimal tongThu, decimal tongChi)> GetBaoCaoTheoNgay(int taiKhoanId, DateTime tuNgay, DateTime denNgay)
        {
            var list = new List<(DateTime, decimal, decimal)>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        NgayGD as Ngay,
                        SUM(CASE WHEN LoaiGD = 'THU' THEN SoTien ELSE 0 END) as TongThu,
                        SUM(CASE WHEN LoaiGD = 'CHI' THEN SoTien ELSE 0 END) as TongChi
                    FROM GiaoDich
                    WHERE TaiKhoanId = @TaiKhoanId 
                    AND NgayGD >= @TuNgay 
                    AND NgayGD <= @DenNgay 
                    AND DaXoa = 0
                    GROUP BY NgayGD
                    ORDER BY NgayGD";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((
                            Convert.ToDateTime(reader["Ngay"]),
                            Convert.ToDecimal(reader["TongThu"]),
                            Convert.ToDecimal(reader["TongChi"])
                        ));
                    }
                }
            }
            return list;
        }

        public List<(int nam, int thang, decimal tongThu, decimal tongChi)> GetBaoCaoTheoThang(int taiKhoanId, int nam)
        {
            var list = new List<(int, int, decimal, decimal)>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        YEAR(NgayGD) as Nam,
                        MONTH(NgayGD) as Thang,
                        SUM(CASE WHEN LoaiGD = 'THU' THEN SoTien ELSE 0 END) as TongThu,
                        SUM(CASE WHEN LoaiGD = 'CHI' THEN SoTien ELSE 0 END) as TongChi
                    FROM GiaoDich
                    WHERE TaiKhoanId = @TaiKhoanId 
                    AND YEAR(NgayGD) = @Nam 
                    AND DaXoa = 0
                    GROUP BY YEAR(NgayGD), MONTH(NgayGD)
                    ORDER BY YEAR(NgayGD), MONTH(NgayGD)";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@Nam", nam));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((
                            Convert.ToInt32(reader["Nam"]),
                            Convert.ToInt32(reader["Thang"]),
                            Convert.ToDecimal(reader["TongThu"]),
                            Convert.ToDecimal(reader["TongChi"])
                        ));
                    }
                }
            }
            return list;
        }

        public List<(int nganSachId, int danhMucId, string tenDanhMuc, decimal gioiHan, decimal daChiTieu, DateTime tgBatDau, DateTime tgKetThuc, string trangThai)> GetSoSanhNganSach(int taiKhoanId, DateTime tuNgay, DateTime denNgay)
        {
            var list = new List<(int, int, string, decimal, decimal, DateTime, DateTime, string)>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        ns.Id as NganSachId,
                        ns.DanhMucId,
                        dm.TenDanhMuc,
                        ns.SoTienGioiHan as GioiHan,
                        ISNULL(SUM(gd.SoTien), 0) as DaChiTieu,
                        ns.TGBatDau,
                        ns.TGKetThuc,
                        ns.TrangThai
                    FROM NganSach ns
                    INNER JOIN DanhMuc dm ON ns.DanhMucId = dm.Id
                    LEFT JOIN GiaoDich gd ON ns.DanhMucId = gd.DanhMucId 
                        AND gd.LoaiGD = 'CHI'
                        AND gd.NgayGD >= ns.TGBatDau 
                        AND gd.NgayGD <= ns.TGKetThuc 
                        AND gd.DaXoa = 0
                    WHERE ns.TaiKhoanId = @TaiKhoanId 
                    AND ns.DaXoa = 0
                    AND (
                        (ns.TGBatDau >= @TuNgay AND ns.TGBatDau <= @DenNgay)
                        OR (ns.TGKetThuc >= @TuNgay AND ns.TGKetThuc <= @DenNgay)
                        OR (ns.TGBatDau <= @TuNgay AND ns.TGKetThuc >= @DenNgay)
                    )
                    GROUP BY ns.Id, ns.DanhMucId, dm.TenDanhMuc, ns.SoTienGioiHan, ns.TGBatDau, ns.TGKetThuc, ns.TrangThai";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((
                            Convert.ToInt32(reader["NganSachId"]),
                            Convert.ToInt32(reader["DanhMucId"]),
                            reader["TenDanhMuc"].ToString(),
                            Convert.ToDecimal(reader["GioiHan"]),
                            Convert.ToDecimal(reader["DaChiTieu"]),
                            Convert.ToDateTime(reader["TGBatDau"]),
                            Convert.ToDateTime(reader["TGKetThuc"]),
                            reader["TrangThai"].ToString()
                        ));
                    }
                }
            }
            return list;
        }

        public List<(int giaoDichId, DateTime ngayGD, string tenDanhMuc, string tenVi, decimal soTien, string loaiGD, string ghiChu)> GetTopGiaoDich(int taiKhoanId, DateTime tuNgay, DateTime denNgay, string loaiGD, int top)
        {
            var list = new List<(int, DateTime, string, string, decimal, string, string)>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT TOP (@Top)
                        gd.Id as GiaoDichId,
                        gd.NgayGD,
                        dm.TenDanhMuc,
                        v.TenVi,
                        gd.SoTien,
                        gd.LoaiGD,
                        gd.GhiChu
                    FROM GiaoDich gd
                    INNER JOIN DanhMuc dm ON gd.DanhMucId = dm.Id
                    INNER JOIN Vi v ON gd.ViId = v.Id
                    WHERE gd.TaiKhoanId = @TaiKhoanId 
                    AND gd.NgayGD >= @TuNgay 
                    AND gd.NgayGD <= @DenNgay 
                    AND gd.DaXoa = 0
                    " + (string.IsNullOrEmpty(loaiGD) ? "" : "AND gd.LoaiGD = @LoaiGD") + @"
                    ORDER BY gd.SoTien DESC";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay));
                cmd.Parameters.Add(new SqlParameter("@Top", top));
                if (!string.IsNullOrEmpty(loaiGD))
                    cmd.Parameters.Add(new SqlParameter("@LoaiGD", loaiGD));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((
                            Convert.ToInt32(reader["GiaoDichId"]),
                            Convert.ToDateTime(reader["NgayGD"]),
                            reader["TenDanhMuc"].ToString(),
                            reader["TenVi"].ToString(),
                            Convert.ToDecimal(reader["SoTien"]),
                            reader["LoaiGD"].ToString(),
                            reader["GhiChu"] == DBNull.Value ? "" : reader["GhiChu"].ToString()
                        ));
                    }
                }
            }
            return list;
        }

        public List<GiaoDich> GetGiaoDichTheoFilter(int taiKhoanId, DateTime tuNgay, DateTime denNgay, int? viId, int? danhMucId, string loaiGD)
        {
            var list = new List<GiaoDich>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT * FROM GiaoDich 
                    WHERE TaiKhoanId = @TaiKhoanId 
                    AND NgayGD >= @TuNgay 
                    AND NgayGD <= @DenNgay 
                    AND DaXoa = 0
                    " + (viId.HasValue ? "AND ViId = @ViId" : "") +
                    (danhMucId.HasValue ? "AND DanhMucId = @DanhMucId" : "") +
                    (string.IsNullOrEmpty(loaiGD) ? "" : "AND LoaiGD = @LoaiGD") + @"
                    ORDER BY NgayGD DESC";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay));
                if (viId.HasValue) cmd.Parameters.Add(new SqlParameter("@ViId", viId.Value));
                if (danhMucId.HasValue) cmd.Parameters.Add(new SqlParameter("@DanhMucId", danhMucId.Value));
                if (!string.IsNullOrEmpty(loaiGD)) cmd.Parameters.Add(new SqlParameter("@LoaiGD", loaiGD));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new GiaoDich
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            TaiKhoanId = Convert.ToInt32(reader["TaiKhoanId"]),
                            ViId = Convert.ToInt32(reader["ViId"]),
                            DanhMucId = Convert.ToInt32(reader["DanhMucId"]),
                            SoTien = Convert.ToDecimal(reader["SoTien"]),
                            LoaiGD = reader["LoaiGD"].ToString(),
                            NgayGD = Convert.ToDateTime(reader["NgayGD"]),
                            GhiChu = reader["GhiChu"] == DBNull.Value ? "" : reader["GhiChu"].ToString(),
                            DaXoa = Convert.ToBoolean(reader["DaXoa"])
                        });
                    }
                }
            }
            return list;
        }
    }
}