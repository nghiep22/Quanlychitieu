using QL_NganQuy.DTO;
using QL_NganQuy.Repositories;

namespace QL_NganQuy.Services
{
    public class BaoCaoService : IBaoCaoService
    {
        private readonly IBaoCaoRepository _repository;

        public BaoCaoService(IBaoCaoRepository repository)
        {
            _repository = repository;
        }

        public TongQuanThuChiDto GetTongQuanThuChi(BaoCaoFilterDto filter)
        {
            var (tongThu, tongChi, soGDThu, soGDChi) = _repository.GetTongQuanThuChi(
                filter.TaiKhoanId,
                filter.TuNgay,
                filter.DenNgay,
                filter.ViId,
                filter.DanhMucId
            );

            return new TongQuanThuChiDto
            {
                TongThu = tongThu,
                TongChi = tongChi,
                ChenhLech = tongThu - tongChi,
                SoGiaoDichThu = soGDThu,
                SoGiaoDichChi = soGDChi
            };
        }

        public List<BaoCaoTheoDanhMucDto> GetBaoCaoTheoDanhMuc(BaoCaoFilterDto filter)
        {
            var data = _repository.GetBaoCaoTheoDanhMuc(
                filter.TaiKhoanId,
                filter.TuNgay,
                filter.DenNgay,
                filter.LoaiGD
            );

            var tongTienAll = data.Sum(x => x.tongTien);

            return data.Select(x => new BaoCaoTheoDanhMucDto
            {
                DanhMucId = x.danhMucId,
                TenDanhMuc = x.tenDanhMuc,
                Loai = x.loai,
                TongTien = x.tongTien,
                SoGiaoDich = x.soGD,
                TiLePhanTram = tongTienAll > 0 ? (x.tongTien / tongTienAll) * 100 : 0
            }).ToList();
        }

        public List<BaoCaoTheoViDto> GetBaoCaoTheoVi(int taiKhoanId, DateTime tuNgay, DateTime denNgay)
        {
            var data = _repository.GetBaoCaoTheoVi(taiKhoanId, tuNgay, denNgay);

            return data.Select(x => new BaoCaoTheoViDto
            {
                ViId = x.viId,
                TenVi = x.tenVi,
                LoaiVi = x.loaiVi,
                SoDuBanDau = x.soDuBanDau,
                TongThu = x.tongThu,
                TongChi = x.tongChi,
                SoDuHienTai = x.soDuBanDau + x.tongThu - x.tongChi
            }).ToList();
        }

        public List<BaoCaoTheoNgayDto> GetBaoCaoTheoNgay(int taiKhoanId, DateTime tuNgay, DateTime denNgay)
        {
            var data = _repository.GetBaoCaoTheoNgay(taiKhoanId, tuNgay, denNgay);

            return data.Select(x => new BaoCaoTheoNgayDto
            {
                Ngay = x.ngay,
                TongThu = x.tongThu,
                TongChi = x.tongChi,
                ChenhLech = x.tongThu - x.tongChi
            }).ToList();
        }

        public List<BaoCaoTheoThangDto> GetBaoCaoTheoThang(int taiKhoanId, int nam)
        {
            var data = _repository.GetBaoCaoTheoThang(taiKhoanId, nam);

            return data.Select(x => new BaoCaoTheoThangDto
            {
                Nam = x.nam,
                Thang = x.thang,
                TongThu = x.tongThu,
                TongChi = x.tongChi,
                ChenhLech = x.tongThu - x.tongChi
            }).ToList();
        }

        public List<SoSanhNganSachDto> GetSoSanhNganSach(int taiKhoanId, DateTime tuNgay, DateTime denNgay)
        {
            var data = _repository.GetSoSanhNganSach(taiKhoanId, tuNgay, denNgay);

            return data.Select(x => new SoSanhNganSachDto
            {
                NganSachId = x.nganSachId,
                DanhMucId = x.danhMucId,
                TenDanhMuc = x.tenDanhMuc,
                SoTienGioiHan = x.gioiHan,
                DaChiTieu = x.daChiTieu,
                ConLai = x.gioiHan - x.daChiTieu,
                TiLeSuDung = x.gioiHan > 0 ? (x.daChiTieu / x.gioiHan) * 100 : 0,
                TrangThai = x.trangThai,
                TGBatDau = x.tgBatDau,
                TGKetThuc = x.tgKetThuc
            }).ToList();
        }

        public List<TopGiaoDichDto> GetTopGiaoDich(int taiKhoanId, DateTime tuNgay, DateTime denNgay, string loaiGD, int top)
        {
            var data = _repository.GetTopGiaoDich(taiKhoanId, tuNgay, denNgay, loaiGD, top);

            return data.Select(x => new TopGiaoDichDto
            {
                GiaoDichId = x.giaoDichId,
                NgayGD = x.ngayGD,
                TenDanhMuc = x.tenDanhMuc,
                TenVi = x.tenVi,
                SoTien = x.soTien,
                LoaiGD = x.loaiGD,
                GhiChu = x.ghiChu
            }).ToList();
        }

        public BaoCaoTongHopDto GetBaoCaoTongHop(int taiKhoanId, DateTime tuNgay, DateTime denNgay)
        {
            var filter = new BaoCaoFilterDto
            {
                TaiKhoanId = taiKhoanId,
                TuNgay = tuNgay,
                DenNgay = denNgay
            };

            return new BaoCaoTongHopDto
            {
                TongQuan = GetTongQuanThuChi(filter),
                TheoDanhMuc = GetBaoCaoTheoDanhMuc(filter),
                TheoVi = GetBaoCaoTheoVi(taiKhoanId, tuNgay, denNgay),
                TheoNgay = GetBaoCaoTheoNgay(taiKhoanId, tuNgay, denNgay),
                TopChiTieuLon = GetTopGiaoDich(taiKhoanId, tuNgay, denNgay, "CHI", 10),
                SoSanhNganSach = GetSoSanhNganSach(taiKhoanId, tuNgay, denNgay)
            };
        }
    }
}