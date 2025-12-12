namespace Models
{
    public class DanhMuc
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public string TenDanhMuc { get; set; } = "";
        public string Loai { get; set; } = ""; // "THU" | "CHI"
        public string? Icon { get; set; }
        public string? MauSac { get; set; }
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = "Hoạt động"; // "Hoạt động" | "Khóa"
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }
}
