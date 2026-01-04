using QL_NganQuy.DTO;

namespace QL_NganQuy.Services
{
    public interface IBaoCaoService
    {
        TongQuanThuChiDto GetTongQuanThuChi(BaoCaoFilterDto filter);
        List<BaoCaoTheoDanhMucDto> GetBaoCaoTheoDanhMuc(BaoCaoFilterDto filter);
        List<BaoCaoTheoViDto> GetBaoCaoTheoVi(int taiKhoanId, DateTime tuNgay, DateTime denNgay);
        List<BaoCaoTheoNgayDto> GetBaoCaoTheoNgay(int taiKhoanId, DateTime tuNgay, DateTime denNgay);
        List<BaoCaoTheoThangDto> GetBaoCaoTheoThang(int taiKhoanId, int nam);
        List<SoSanhNganSachDto> GetSoSanhNganSach(int taiKhoanId, DateTime tuNgay, DateTime denNgay);
        List<TopGiaoDichDto> GetTopGiaoDich(int taiKhoanId, DateTime tuNgay, DateTime denNgay, string loaiGD, int top);
        BaoCaoTongHopDto GetBaoCaoTongHop(int taiKhoanId, DateTime tuNgay, DateTime denNgay);
    }
}