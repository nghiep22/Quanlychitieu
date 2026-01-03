namespace Models
{
    public class DongGopMucTieu
    {
        public int Id { get; set; }
        public int MucTieuId { get; set; }
        public int TaiKhoanId { get; set; }
        public int? GiaoDichId { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayDongGop { get; set; }
        public string? GhiChu { get; set; }
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class DongGopMucTieuCreateRequest
    {
        public int MucTieuId { get; set; }
        public int TaiKhoanId { get; set; }
        public int? GiaoDichId { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayDongGop { get; set; }  // gửi "2025-12-24"
        public string? GhiChu { get; set; }
    }

    public class DongGopMucTieuUpdateRequest
    {
        public decimal SoTien { get; set; }
        public DateTime NgayDongGop { get; set; }
        public string? GhiChu { get; set; }
    }
}
