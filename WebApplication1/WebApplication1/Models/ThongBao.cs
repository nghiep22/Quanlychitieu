namespace QL_NganQuy.Models
{
    // Class đơn giản, giống bảng ThongBao trong DB
    public class ThongBao
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
}