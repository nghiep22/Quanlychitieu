namespace QL_NganQuy.DTO
{
    // DTO để nhận từ client khi tạo thông báo
    public class CreateThongBaoDto
    {
        public int TaiKhoanId { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string Loai { get; set; }
        public int? GiaoDichId { get; set; }
        public int? NganSachId { get; set; }
        public int? MucTieuId { get; set; }
    }

    // DTO để nhận từ client khi cập nhật
    public class UpdateThongBaoDto
    {
        public string TrangThai { get; set; }
    }

    // DTO để trả về cho client
    public class ThongBaoDto
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string Loai { get; set; }
        public int? GiaoDichId { get; set; }
        public int? NganSachId { get; set; }
        public int? MucTieuId { get; set; }
        public DateTime ThoiGianGui { get; set; }
        public string TrangThai { get; set; }
    }

    // DTO để lọc
    public class ThongBaoFilterDto
    {
        public int TaiKhoanId { get; set; }
        public string Loai { get; set; }
        public string TrangThai { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // DTO phân trang
    public class PagedThongBaoDto
    {
        public List<ThongBaoDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // DTO thống kê
    public class ThongBaoThongKeDto
    {
        public int TongSoThongBao { get; set; }
        public int SoChuaXem { get; set; }
        public int SoDaXem { get; set; }
        public Dictionary<string, int> TheoLoai { get; set; }
    }
}