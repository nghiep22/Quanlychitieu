namespace QL_NganQuy.Models
{
    public class TaiKhoan
    {
        public int Id { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string HoTen { get; set; }
        public string Quyen { get; set; }
        public bool IsActive { get; set; }
    }
}