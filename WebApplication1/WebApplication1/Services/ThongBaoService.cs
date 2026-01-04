using QL_NganQuy.DTO;
using QL_NganQuy.Models;
using QL_NganQuy.Repositories;

namespace QL_NganQuy.Services
{
    public class ThongBaoService : IThongBaoService
    {
        private readonly IThongBaoRepository _repository;

        public ThongBaoService(IThongBaoRepository repository)
        {
            _repository = repository;
        }

        public ThongBaoDto GetById(int id, int taiKhoanId)
        {
            var thongBao = _repository.GetById(id);

            if (thongBao == null || thongBao.TaiKhoanId != taiKhoanId)
                return null;

            return MapToDto(thongBao);
        }

        public List<ThongBaoDto> GetAllByTaiKhoan(int taiKhoanId)
        {
            var thongBaos = _repository.GetByTaiKhoanId(taiKhoanId);
            return thongBaos.Select(MapToDto).ToList();
        }

        public List<ThongBaoDto> GetUnread(int taiKhoanId)
        {
            var thongBaos = _repository.GetUnreadByTaiKhoanId(taiKhoanId);
            return thongBaos.Select(MapToDto).ToList();
        }

        public List<ThongBaoDto> GetByLoai(int taiKhoanId, string loai)
        {
            var thongBaos = _repository.GetByLoai(taiKhoanId, loai);
            return thongBaos.Select(MapToDto).ToList();
        }

        public PagedThongBaoDto GetPaged(ThongBaoFilterDto filter)
        {
            var (items, totalCount) = _repository.GetPaged(
                filter.TaiKhoanId,
                filter.Loai,
                filter.TrangThai,
                filter.TuNgay,
                filter.DenNgay,
                filter.PageNumber,
                filter.PageSize
            );

            return new PagedThongBaoDto
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public ThongBaoThongKeDto GetThongKe(int taiKhoanId)
        {
            var allThongBaos = _repository.GetByTaiKhoanId(taiKhoanId);
            var unreadCount = _repository.CountUnread(taiKhoanId);
            var theoLoai = _repository.GetStatisticsByLoai(taiKhoanId);

            return new ThongBaoThongKeDto
            {
                TongSoThongBao = allThongBaos.Count,
                SoChuaXem = unreadCount,
                SoDaXem = allThongBaos.Count - unreadCount,
                TheoLoai = theoLoai
            };
        }

        public ThongBaoDto Create(CreateThongBaoDto dto)
        {
            if (!IsValidLoai(dto.Loai))
                throw new ArgumentException("Loại thông báo không hợp lệ");

            var thongBao = new ThongBao
            {
                TaiKhoanId = dto.TaiKhoanId,
                TieuDe = dto.TieuDe,
                NoiDung = dto.NoiDung,
                Loai = dto.Loai,
                GiaoDichId = dto.GiaoDichId,
                NganSachId = dto.NganSachId,
                MucTieuId = dto.MucTieuId,
                ThoiGianGui = DateTime.Now,
                TrangThai = "Chưa xem"
            };

            var id = _repository.Create(thongBao);
            thongBao.Id = id;

            return MapToDto(thongBao);
        }

        public ThongBaoDto Update(int id, UpdateThongBaoDto dto, int taiKhoanId)
        {
            var thongBao = _repository.GetById(id);

            if (thongBao == null || thongBao.TaiKhoanId != taiKhoanId)
                return null;

            if (!string.IsNullOrEmpty(dto.TrangThai) && !IsValidTrangThai(dto.TrangThai))
                throw new ArgumentException("Trạng thái không hợp lệ");

            if (!string.IsNullOrEmpty(dto.TrangThai))
                thongBao.TrangThai = dto.TrangThai;

            _repository.Update(thongBao);
            return MapToDto(thongBao);
        }

        public bool Delete(int id, int taiKhoanId)
        {
            var thongBao = _repository.GetById(id);

            if (thongBao == null || thongBao.TaiKhoanId != taiKhoanId)
                return false;

            return _repository.Delete(id);
        }

        public bool MarkAsRead(int id, int taiKhoanId)
        {
            var thongBao = _repository.GetById(id);

            if (thongBao == null || thongBao.TaiKhoanId != taiKhoanId)
                return false;

            return _repository.MarkAsRead(id);
        }

        public bool MarkAllAsRead(int taiKhoanId)
        {
            return _repository.MarkAllAsRead(taiKhoanId);
        }

        public void CreateNotificationForBudgetWarning(int taiKhoanId, int nganSachId, decimal chiTieu, decimal gioiHan)
        {
            var tiLe = (chiTieu / gioiHan) * 100;
            string loai = "Cảnh báo";
            string tieuDe = "Cảnh báo ngân sách";
            string noiDung = $"Bạn đã chi tiêu {chiTieu:N0} VNĐ / {gioiHan:N0} VNĐ ({tiLe:N1}%) ngân sách.";

            if (tiLe >= 100)
            {
                loai = "Lỗi";
                tieuDe = "Vượt ngân sách";
                noiDung = $"Bạn đã vượt ngân sách! Chi tiêu: {chiTieu:N0} VNĐ / {gioiHan:N0} VNĐ.";
            }
            else if (tiLe >= 80)
            {
                noiDung = $"Cảnh báo: Bạn đã sử dụng {tiLe:N1}% ngân sách.";
            }

            var dto = new CreateThongBaoDto
            {
                TaiKhoanId = taiKhoanId,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                Loai = loai,
                NganSachId = nganSachId
            };

            Create(dto);
        }

        public void CreateNotificationForGoalProgress(int taiKhoanId, int mucTieuId, decimal tiLe)
        {
            string loai = "Thông tin";
            string tieuDe = "Tiến độ mục tiêu";
            string noiDung = $"Bạn đã đạt {tiLe:N1}% mục tiêu tiết kiệm.";

            if (tiLe >= 100)
            {
                loai = "Thành công";
                tieuDe = "Hoàn thành mục tiêu";
                noiDung = "Chúc mừng! Bạn đã hoàn thành mục tiêu tiết kiệm.";
            }
            else if (tiLe >= 75)
            {
                loai = "Thông tin";
                noiDung = $"Tuyệt vời! Bạn đã đạt {tiLe:N1}% mục tiêu.";
            }

            var dto = new CreateThongBaoDto
            {
                TaiKhoanId = taiKhoanId,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                Loai = loai,
                MucTieuId = mucTieuId
            };

            Create(dto);
        }

        public void CreateNotificationForTransaction(int taiKhoanId, int giaoDichId, string message)
        {
            var dto = new CreateThongBaoDto
            {
                TaiKhoanId = taiKhoanId,
                TieuDe = "Giao dịch mới",
                NoiDung = message,
                Loai = "Thông tin",
                GiaoDichId = giaoDichId
            };

            Create(dto);
        }

        public int CleanupOldNotifications(int taiKhoanId, int daysOld = 30)
        {
            return _repository.DeleteOldNotifications(taiKhoanId, daysOld);
        }

        private ThongBaoDto MapToDto(ThongBao thongBao)
        {
            return new ThongBaoDto
            {
                Id = thongBao.Id,
                TaiKhoanId = thongBao.TaiKhoanId,
                TieuDe = thongBao.TieuDe,
                NoiDung = thongBao.NoiDung,
                Loai = thongBao.Loai,
                GiaoDichId = thongBao.GiaoDichId,
                NganSachId = thongBao.NganSachId,
                MucTieuId = thongBao.MucTieuId,
                ThoiGianGui = thongBao.ThoiGianGui,
                TrangThai = thongBao.TrangThai
            };
        }

        private bool IsValidLoai(string loai)
        {
            var validLoai = new[] { "Cảnh báo", "Thông tin", "Thành công", "Lỗi" };
            return validLoai.Contains(loai);
        }

        private bool IsValidTrangThai(string trangThai)
        {
            var validTrangThai = new[] { "Chưa xem", "Đã xem" };
            return validTrangThai.Contains(trangThai);
        }
    }
}