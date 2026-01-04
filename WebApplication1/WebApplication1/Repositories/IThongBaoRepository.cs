using QL_NganQuy.Models;

namespace QL_NganQuy.Repositories
{
    public interface IThongBaoRepository
    {
        ThongBao GetById(int id);
        List<ThongBao> GetAll();
        List<ThongBao> GetByTaiKhoanId(int taiKhoanId);
        List<ThongBao> GetUnreadByTaiKhoanId(int taiKhoanId);
        List<ThongBao> GetByLoai(int taiKhoanId, string loai);
        (List<ThongBao> items, int totalCount) GetPaged(int taiKhoanId, string loai, string trangThai, DateTime? tuNgay, DateTime? denNgay, int pageNumber, int pageSize);

        int Create(ThongBao thongBao);
        bool Update(ThongBao thongBao);
        bool Delete(int id);

        bool MarkAsRead(int id);
        bool MarkAllAsRead(int taiKhoanId);

        int CountUnread(int taiKhoanId);
        Dictionary<string, int> GetStatisticsByLoai(int taiKhoanId);
        int DeleteOldNotifications(int taiKhoanId, int daysOld);
    }
}