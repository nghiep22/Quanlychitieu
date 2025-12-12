namespace Models
{
    public class DanhMucCreateRequest
    {
        public int TaiKhoanId { get; set; }
        public string TenDanhMuc { get; set; } = "";
        public string Loai { get; set; } = ""; // THU/CHI
        public string? Icon { get; set; }
        public string? MauSac { get; set; }
        public string? GhiChu { get; set; }
    }

    public class DanhMucUpdateRequest
    {
        public int TaiKhoanId { get; set; }
        public string TenDanhMuc { get; set; } = "";
        public string Loai { get; set; } = ""; // THU/CHI
        public string? Icon { get; set; }
        public string? MauSac { get; set; }
        public string? GhiChu { get; set; }
    }

    public class DanhMucLockRequest
    {
        public int TaiKhoanId { get; set; }
        public bool IsLocked { get; set; } // true=Khóa, false=Hoạt động
    }
}
