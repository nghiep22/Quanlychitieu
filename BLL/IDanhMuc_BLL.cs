using Models;

namespace BLL
{
    public interface IDanhMuc_BLL
    {
        List<DanhMuc> GetAll(int taiKhoanId, string? loai, string? status, bool includeDeleted);
        DanhMuc? GetById(int id, int taiKhoanId, bool includeDeleted);
        int Create(DanhMucCreateRequest req);
        bool Update(int id, DanhMucUpdateRequest req);
        bool Lock(int id, DanhMucLockRequest req);
        bool Delete(int id, int taiKhoanId);
    }
}
