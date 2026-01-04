namespace QL_NganQuy.DTO
{
    // DTO tạo tài khoản
    public class CreateTaiKhoanDto
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string HoTen { get; set; }
        public string Quyen { get; set; }
    }

    // DTO cập nhật tài khoản
    public class UpdateTaiKhoanDto
    {
        public string? HoTen { get; set; }
        public string? Quyen { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO trả về (không có mật khẩu)
    public class TaiKhoanDto
    {
        public int Id { get; set; }
        public string TenDangNhap { get; set; }
        public string HoTen { get; set; }
        public string Quyen { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO tìm kiếm
    public class TaiKhoanFilterDto
    {
        public string Quyen { get; set; }
        public bool? IsActive { get; set; }
    }
}