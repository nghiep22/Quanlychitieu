namespace Models
{
    public class ThongBao
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public string TieuDe { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public string Loai { get; set; } = "";          // Cảnh báo / Thông tin / Thành công / Lỗi
        public int? GiaoDichId { get; set; }
        public int? NganSachId { get; set; }
        public int? MucTieuId { get; set; }
        public DateTime ThoiGianGui { get; set; }
        public string TrangThai { get; set; } = "";     // Chưa xem / Đã xem
    }

    public class ThongBaoCreateRequest
    {
        public int TaiKhoanId { get; set; }
        public string TieuDe { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public string Loai { get; set; } = "";          // Cảnh báo / Thông tin / Thành công / Lỗi
        public int? GiaoDichId { get; set; }
        public int? NganSachId { get; set; }
        public int? MucTieuId { get; set; }
    }

    public class ThongBaoUpdateTrangThaiRequest
    {
        public int TaiKhoanId { get; set; }
        public bool DaXem { get; set; } // true => "Đã xem", false => "Chưa xem"
    }
}
