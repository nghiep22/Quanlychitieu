namespace QL_NganQuy.DTO
{
    // DTO tạo ví
    public class CreateViDto
    {
        public int TaiKhoanId { get; set; }
        public string TenVi { get; set; }
        public string LoaiVi { get; set; }
        public decimal SoDuBanDau { get; set; }
        public string GhiChu { get; set; }
    }

    // DTO cập nhật ví
    public class UpdateViDto
    {
        public string? TenVi { get; set; }
        public string? LoaiVi { get; set; }
        public string? GhiChu { get; set; }
        public string? TrangThai { get; set; }
    }

    // DTO trả về
    public class ViDto
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public string TenVi { get; set; }
        public string LoaiVi { get; set; }
        public decimal SoDuBanDau { get; set; }
        public string GhiChu { get; set; }
        public string TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    // DTO tìm kiếm
    public class ViFilterDto
    {
        public int TaiKhoanId { get; set; }
        public string LoaiVi { get; set; }
        public string TrangThai { get; set; }
    }
}