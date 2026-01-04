using QL_NganQuy.DTO;

namespace QL_NganQuy.Services
{
    public interface IThongBaoService
    {
        ThongBaoDto GetById(int id, int taiKhoanId);
        List<ThongBaoDto> GetAllByTaiKhoan(int taiKhoanId);
        List<ThongBaoDto> GetUnread(int taiKhoanId);
        List<ThongBaoDto> GetByLoai(int taiKhoanId, string loai);
        PagedThongBaoDto GetPaged(ThongBaoFilterDto filter);
        ThongBaoThongKeDto GetThongKe(int taiKhoanId);

        ThongBaoDto Create(CreateThongBaoDto dto);
        ThongBaoDto Update(int id, UpdateThongBaoDto dto, int taiKhoanId);
        bool Delete(int id, int taiKhoanId);

        bool MarkAsRead(int id, int taiKhoanId);
        bool MarkAllAsRead(int taiKhoanId);

        void CreateNotificationForBudgetWarning(int taiKhoanId, int nganSachId, decimal chiTieu, decimal gioiHan);
        void CreateNotificationForGoalProgress(int taiKhoanId, int mucTieuId, decimal tiLe);
        void CreateNotificationForTransaction(int taiKhoanId, int giaoDichId, string message);

        int CleanupOldNotifications(int taiKhoanId, int daysOld = 30);
    }
}