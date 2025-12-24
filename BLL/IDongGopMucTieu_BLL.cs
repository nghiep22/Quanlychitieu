using Models;

namespace BLL
{
    public interface IDongGopMucTieu_BLL
    {
        Task<List<DongGopMucTieu>> GetByMucTieuAsync(int mucTieuId, bool includeDeleted);
        Task<DongGopMucTieu?> GetByIdAsync(int id, int taiKhoanId, bool includeDeleted);
        Task<int> CreateAsync(DongGopMucTieuCreateRequest req);
        Task<bool> UpdateAsync(int id, int taiKhoanId, DongGopMucTieuUpdateRequest req);
        Task<bool> SoftDeleteAsync(int id, int taiKhoanId);
    }
}
