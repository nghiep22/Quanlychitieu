namespace Models
{
    public class GiaoDich
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public int ViId { get; set; }
        public int DanhMucId { get; set; }
        public decimal SoTien { get; set; }
        public string LoaiGD { get; set; } = "";   // THU/CHI
        public DateTime NgayGD { get; set; }       // DATE -> map DateTime
        public string? GhiChu { get; set; }
        public string? AnhHoaDon { get; set; }
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }
}
