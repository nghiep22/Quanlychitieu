namespace Models
{
    public class TaiKhoan
    {
        public int Id { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty; // DEMO: mật khẩu thô
        public string? HoTen { get; set; }
        public string? Quyen { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
