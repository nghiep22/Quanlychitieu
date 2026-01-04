using QL_NganQuy.Models;

namespace QL_NganQuy.Repositories
{
    public interface IBaoCaoRepository
    {
        // Lấy tổng thu chi
        (decimal tongThu, decimal tongChi, int soGDThu, int soGDChi) GetTongQuanThuChi(int taiKhoanId, DateTime tuNgay, DateTime denNgay, int? viId, int? danhMucId);

        // Báo cáo theo danh mục
        List<(int danhMucId, string tenDanhMuc, string loai, decimal tongTien, int soGD)> GetBaoCaoTheoDanhMuc(int taiKhoanId, DateTime tuNgay, DateTime denNgay, string loaiGD);

        // Báo cáo theo ví
        List<(int viId, string tenVi, string loaiVi, decimal soDuBanDau, decimal tongThu, decimal tongChi)> GetBaoCaoTheoVi(int taiKhoanId, DateTime tuNgay, DateTime denNgay);

        // Báo cáo theo ngày
        List<(DateTime ngay, decimal tongThu, decimal tongChi)> GetBaoCaoTheoNgay(int taiKhoanId, DateTime tuNgay, DateTime denNgay);

        // Báo cáo theo tháng
        List<(int nam, int thang, decimal tongThu, decimal tongChi)> GetBaoCaoTheoThang(int taiKhoanId, int nam);

        // So sánh ngân sách
        List<(int nganSachId, int danhMucId, string tenDanhMuc, decimal gioiHan, decimal daChiTieu, DateTime tgBatDau, DateTime tgKetThuc, string trangThai)> GetSoSanhNganSach(int taiKhoanId, DateTime tuNgay, DateTime denNgay);

        // Top giao dịch chi tiêu lớn
        List<(int giaoDichId, DateTime ngayGD, string tenDanhMuc, string tenVi, decimal soTien, string loaiGD, string ghiChu)> GetTopGiaoDich(int taiKhoanId, DateTime tuNgay, DateTime denNgay, string loaiGD, int top);

        // Lấy danh sách giao dịch theo bộ lọc
        List<GiaoDich> GetGiaoDichTheoFilter(int taiKhoanId, DateTime tuNgay, DateTime denNgay, int? viId, int? danhMucId, string loaiGD);
    }
}