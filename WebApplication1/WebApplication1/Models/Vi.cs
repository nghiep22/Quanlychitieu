namespace QL_NganQuy.Models
{
    public class Vi
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public string TenVi { get; set; }
        public string LoaiVi { get; set; }
        public decimal SoDuBanDau { get; set; }
        public string GhiChu { get; set; }
        public string TrangThai { get; set; }
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }
}