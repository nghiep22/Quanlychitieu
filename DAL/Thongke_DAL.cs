using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Models.Reports;

namespace DAL
{
    public class Reports_DAL
    {
        private readonly string _connStr;

        public Reports_DAL(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Missing DefaultConnection");
        }

        public ReportSummaryDto GetSummary(int taiKhoanId, int month, int year, int? viId)
        {
            const string sql = @"
SELECT
    SUM(CASE WHEN LoaiGD='THU' THEN SoTien ELSE 0 END) AS TongThu,
    SUM(CASE WHEN LoaiGD='CHI' THEN SoTien ELSE 0 END) AS TongChi
FROM dbo.GiaoDich
WHERE TaiKhoanId = @taiKhoanId
  AND DaXoa = 0
  AND YEAR(NgayGD) = @year
  AND MONTH(NgayGD) = @month
  AND (@viId IS NULL OR ViId = @viId);
";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@taiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@month", month);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@viId", (object?)viId ?? DBNull.Value);

            conn.Open();
            using var rd = cmd.ExecuteReader();

            var dto = new ReportSummaryDto();
            if (rd.Read())
            {
                dto.TongThu = rd["TongThu"] == DBNull.Value ? 0 : (decimal)rd["TongThu"];
                dto.TongChi = rd["TongChi"] == DBNull.Value ? 0 : (decimal)rd["TongChi"];
            }
            return dto;
        }

        public List<CategoryReportDto> GetByCategory(int taiKhoanId, int month, int year, int? viId)
        {
            const string sql = @"
SELECT
    dm.Id AS DanhMucId,
    dm.TenDanhMuc,
    dm.Loai,
    SUM(gd.SoTien) AS TongTien
FROM dbo.GiaoDich gd
JOIN dbo.DanhMuc dm
  ON dm.Id = gd.DanhMucId
 AND dm.TaiKhoanId = gd.TaiKhoanId
WHERE gd.TaiKhoanId = @taiKhoanId
  AND gd.DaXoa = 0
  AND YEAR(gd.NgayGD) = @year
  AND MONTH(gd.NgayGD) = @month
  AND (@viId IS NULL OR gd.ViId = @viId)
GROUP BY dm.Id, dm.TenDanhMuc, dm.Loai
ORDER BY dm.Loai, TongTien DESC;
";
            var list = new List<CategoryReportDto>();

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@taiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@month", month);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@viId", (object?)viId ?? DBNull.Value);

            conn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new CategoryReportDto
                {
                    DanhMucId = (int)rd["DanhMucId"],
                    TenDanhMuc = rd["TenDanhMuc"].ToString() ?? "",
                    Loai = rd["Loai"].ToString() ?? "",
                    TongTien = rd["TongTien"] == DBNull.Value ? 0 : (decimal)rd["TongTien"]
                });
            }
            return list;
        }

        public List<WalletReportDto> GetByWallet(int taiKhoanId, int month, int year)
        {
            // Lấy theo ví (kể cả ví chưa có giao dịch tháng đó -> vẫn hiện)
            const string sql = @"
SELECT
    v.Id AS ViId,
    v.TenVi,
    SUM(CASE WHEN gd.LoaiGD='THU' THEN gd.SoTien ELSE 0 END) AS TongThu,
    SUM(CASE WHEN gd.LoaiGD='CHI' THEN gd.SoTien ELSE 0 END) AS TongChi,
    SUM(CASE WHEN gd.LoaiGD='THU' THEN gd.SoTien ELSE -gd.SoTien END) AS ChenhLech
FROM dbo.Vi v
LEFT JOIN dbo.GiaoDich gd
  ON gd.ViId = v.Id
 AND gd.TaiKhoanId = v.TaiKhoanId
 AND gd.DaXoa = 0
 AND YEAR(gd.NgayGD) = @year
 AND MONTH(gd.NgayGD) = @month
WHERE v.TaiKhoanId = @taiKhoanId
  AND v.DaXoa = 0
GROUP BY v.Id, v.TenVi
ORDER BY v.TenVi;
";
            var list = new List<WalletReportDto>();

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@taiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@month", month);
            cmd.Parameters.AddWithValue("@year", year);

            conn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new WalletReportDto
                {
                    ViId = (int)rd["ViId"],
                    TenVi = rd["TenVi"].ToString() ?? "",
                    TongThu = rd["TongThu"] == DBNull.Value ? 0 : (decimal)rd["TongThu"],
                    TongChi = rd["TongChi"] == DBNull.Value ? 0 : (decimal)rd["TongChi"],
                    ChenhLech = rd["ChenhLech"] == DBNull.Value ? 0 : (decimal)rd["ChenhLech"]
                });
            }
            return list;
        }

        public List<CashflowPointDto> GetCashflow(int taiKhoanId, DateTime from, DateTime to, string groupBy, int? viId)
        {
            // groupBy: day | week  (week: lấy ngày đầu tuần, DATEFIRST=1 => Monday)
            const string sql = @"
SET DATEFIRST 1;

WITH x AS
(
    SELECT
        CASE
            WHEN @groupBy = 'week'
                THEN DATEADD(day, 1 - DATEPART(weekday, NgayGD), NgayGD)
            ELSE NgayGD
        END AS NgayNhom,
        SUM(CASE WHEN LoaiGD='THU' THEN SoTien ELSE 0 END) AS TongThu,
        SUM(CASE WHEN LoaiGD='CHI' THEN SoTien ELSE 0 END) AS TongChi
    FROM dbo.GiaoDich
    WHERE TaiKhoanId = @taiKhoanId
      AND DaXoa = 0
      AND NgayGD >= @from AND NgayGD <= @to
      AND (@viId IS NULL OR ViId = @viId)
    GROUP BY
        CASE
            WHEN @groupBy = 'week'
                THEN DATEADD(day, 1 - DATEPART(weekday, NgayGD), NgayGD)
            ELSE NgayGD
        END
)
SELECT
    NgayNhom,
    TongThu,
    TongChi,
    (TongThu - TongChi) AS ChenhLech
FROM x
ORDER BY NgayNhom;
";

            var list = new List<CashflowPointDto>();

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@taiKhoanId", taiKhoanId);
            cmd.Parameters.AddWithValue("@from", from.Date);
            cmd.Parameters.AddWithValue("@to", to.Date);
            cmd.Parameters.AddWithValue("@groupBy", groupBy.ToLower());
            cmd.Parameters.AddWithValue("@viId", (object?)viId ?? DBNull.Value);

            conn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new CashflowPointDto
                {
                    NgayNhom = (DateTime)rd["NgayNhom"],
                    TongThu = rd["TongThu"] == DBNull.Value ? 0 : (decimal)rd["TongThu"],
                    TongChi = rd["TongChi"] == DBNull.Value ? 0 : (decimal)rd["TongChi"],
                    ChenhLech = rd["ChenhLech"] == DBNull.Value ? 0 : (decimal)rd["ChenhLech"]
                });
            }
            return list;
        }
    }
}
