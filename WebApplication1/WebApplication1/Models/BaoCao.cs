namespace QL_NganQuy.Models
{
    // Class để tổng hợp dữ liệu từ nhiều bảng
    // Không phải là bảng trong DB, chỉ dùng để trả kết quả

    public class GiaoDich
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public int ViId { get; set; }
        public int DanhMucId { get; set; }
        public decimal SoTien { get; set; }
        public string LoaiGD { get; set; }
        public DateTime NgayGD { get; set; }
        public string GhiChu { get; set; }
        public bool DaXoa { get; set; }
    }

    public class NganSach
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public int DanhMucId { get; set; }
        public decimal SoTienGioiHan { get; set; }
        public DateTime TGBatDau { get; set; }
        public DateTime TGKetThuc { get; set; }
        public string TrangThai { get; set; }
        public bool DaXoa { get; set; }
    }

    //public class Vi
    //{
    //    public int Id { get; set; }
    //    public int TaiKhoanId { get; set; }
    //    public string TenVi { get; set; }
    //    public string LoaiVi { get; set; }
    //    public decimal SoDuBanDau { get; set; }
    //    public bool DaXoa { get; set; }
    //}

    public class DanhMuc
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public string TenDanhMuc { get; set; }
        public string Loai { get; set; }
        public bool DaXoa { get; set; }
    }
}