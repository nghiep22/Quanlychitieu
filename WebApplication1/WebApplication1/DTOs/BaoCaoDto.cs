namespace QL_NganQuy.DTO
{
    // DTO để lọc báo cáo
    public class BaoCaoFilterDto
    {
        public int TaiKhoanId { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public int? ViId { get; set; }
        public int? DanhMucId { get; set; }
        public string LoaiGD { get; set; } // THU/CHI
    }

    // DTO tổng quan thu chi
    public class TongQuanThuChiDto
    {
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal ChenhLech { get; set; }
        public int SoGiaoDichThu { get; set; }
        public int SoGiaoDichChi { get; set; }
    }

    // DTO chi tiết theo danh mục
    public class BaoCaoTheoDanhMucDto
    {
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; }
        public string Loai { get; set; }
        public decimal TongTien { get; set; }
        public int SoGiaoDich { get; set; }
        public decimal TiLePhanTram { get; set; }
    }

    // DTO chi tiết theo ví
    public class BaoCaoTheoViDto
    {
        public int ViId { get; set; }
        public string TenVi { get; set; }
        public string LoaiVi { get; set; }
        public decimal SoDuBanDau { get; set; }
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal SoDuHienTai { get; set; }
    }

    // DTO báo cáo theo ngày
    public class BaoCaoTheoNgayDto
    {
        public DateTime Ngay { get; set; }
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal ChenhLech { get; set; }
    }

    // DTO báo cáo theo tháng
    public class BaoCaoTheoThangDto
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal ChenhLech { get; set; }
    }

    // DTO so sánh ngân sách
    public class SoSanhNganSachDto
    {
        public int NganSachId { get; set; }
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; }
        public decimal SoTienGioiHan { get; set; }
        public decimal DaChiTieu { get; set; }
        public decimal ConLai { get; set; }
        public decimal TiLeSuDung { get; set; }
        public string TrangThai { get; set; }
        public DateTime TGBatDau { get; set; }
        public DateTime TGKetThuc { get; set; }
    }

    // DTO top giao dịch
    public class TopGiaoDichDto
    {
        public int GiaoDichId { get; set; }
        public DateTime NgayGD { get; set; }
        public string TenDanhMuc { get; set; }
        public string TenVi { get; set; }
        public decimal SoTien { get; set; }
        public string LoaiGD { get; set; }
        public string GhiChu { get; set; }
    }

    // DTO báo cáo tổng hợp đầy đủ
    public class BaoCaoTongHopDto
    {
        public TongQuanThuChiDto TongQuan { get; set; }
        public List<BaoCaoTheoDanhMucDto> TheoDanhMuc { get; set; }
        public List<BaoCaoTheoViDto> TheoVi { get; set; }
        public List<BaoCaoTheoNgayDto> TheoNgay { get; set; }
        public List<TopGiaoDichDto> TopChiTieuLon { get; set; }
        public List<SoSanhNganSachDto> SoSanhNganSach { get; set; }
    }
}
