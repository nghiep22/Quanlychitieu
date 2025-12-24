using DAL;
using Models;

namespace BLL
{
    public class ThongBao_BLL : IThongBao_BLL
    {
        private readonly ThongBao_DAL _dal;

        private static readonly HashSet<string> LOAI_OK = new()
        {
            "Cảnh báo", "Thông tin", "Thành công", "Lỗi"
        };

        private static readonly HashSet<string> TRANGTHAI_OK = new()
        {
            "Chưa xem", "Đã xem"
        };

        public ThongBao_BLL(ThongBao_DAL dal)
        {
            _dal = dal;
        }

        private static void ValidateTaiKhoan(int taiKhoanId)
        {
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");
        }

        private static string? NormalizeLoai(string? loai)
        {
            if (string.IsNullOrWhiteSpace(loai)) return null;
            loai = loai.Trim();
            if (!LOAI_OK.Contains(loai)) throw new ArgumentException("Loai must be: Cảnh báo / Thông tin / Thành công / Lỗi");
            return loai;
        }

        private static string? NormalizeTrangThai(string? trangThai)
        {
            if (string.IsNullOrWhiteSpace(trangThai)) return null;
            trangThai = trangThai.Trim();
            if (!TRANGTHAI_OK.Contains(trangThai)) throw new ArgumentException("TrangThai must be: Chưa xem / Đã xem");
            return trangThai;
        }

        public Task<List<ThongBao>> GetAllAsync(int taiKhoanId, string? loai, string? trangThai, DateTime? from, DateTime? to)
        {
            ValidateTaiKhoan(taiKhoanId);
            loai = NormalizeLoai(loai);
            trangThai = NormalizeTrangThai(trangThai);
            return _dal.GetAllAsync(taiKhoanId, loai, trangThai, from, to);
        }

        public Task<ThongBao?> GetByIdAsync(int id, int taiKhoanId)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            ValidateTaiKhoan(taiKhoanId);
            return _dal.GetByIdAsync(id, taiKhoanId);
        }

        public Task<int> CreateAsync(ThongBaoCreateRequest req)
        {
            if (req == null) throw new ArgumentException("Body is required");
            ValidateTaiKhoan(req.TaiKhoanId);

            if (string.IsNullOrWhiteSpace(req.TieuDe)) throw new ArgumentException("TieuDe is required");
            if (string.IsNullOrWhiteSpace(req.NoiDung)) throw new ArgumentException("NoiDung is required");

            req.Loai = NormalizeLoai(req.Loai) ?? throw new ArgumentException("Loai is required");

            return _dal.CreateAsync(req);
        }

        public Task<bool> UpdateTrangThaiAsync(int id, int taiKhoanId, bool daXem)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            ValidateTaiKhoan(taiKhoanId);

            var trangThai = daXem ? "Đã xem" : "Chưa xem";
            return _dal.UpdateTrangThaiAsync(id, taiKhoanId, trangThai);
        }

        public Task<bool> DeleteAsync(int id, int taiKhoanId)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            ValidateTaiKhoan(taiKhoanId);
            return _dal.DeleteAsync(id, taiKhoanId);
        }
    }
}
