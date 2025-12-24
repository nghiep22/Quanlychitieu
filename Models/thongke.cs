namespace Models.Reports
{
    public class ReportSummaryDto
    {
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal ChenhLech => TongThu - TongChi;
    }

    public class CategoryReportDto
    {
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; } = "";
        public string Loai { get; set; } = ""; // THU/CHI
        public decimal TongTien { get; set; }
    }

    public class WalletReportDto
    {
        public int ViId { get; set; }
        public string TenVi { get; set; } = "";
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal ChenhLech { get; set; }
    }

    public class CashflowPointDto
    {
        public DateTime NgayNhom { get; set; }   // ngày (groupBy=day) hoặc đầu tuần (groupBy=week)
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal ChenhLech { get; set; }
    }
}
