namespace Models
{
    public class ChuyenTienVi
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public int ViNguonId { get; set; }
        public int ViDichId { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayChuyen { get; set; }
        public string? GhiChu { get; set; }

        public int GiaoDichChiId { get; set; }
        public int GiaoDichThuId { get; set; }

        public string TrangThai { get; set; } = "Thành công";
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; }
    }
}