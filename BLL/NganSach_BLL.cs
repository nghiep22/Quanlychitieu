using DAL;
using Microsoft.Data.SqlClient;
using Models;
namespace BLL
{
    public interface INganSach_BLL
    {
        Task<List<NganSach>> GetBudgetsAsync(int taiKhoanId, bool includeDeleted = false);
        Task<int> CreateBudgetAsync(NganSachCreateRequest req);
        Task<bool> UpdateBudgetAsync(int id, int taiKhoanId, NganSachUpdateRequest req);
        Task<bool> LockBudgetAsync(int id, int taiKhoanId, bool isLocked);
        Task<bool> SoftDeleteBudgetAsync(int id, int taiKhoanId);
    }
    public class NganSach_BLL : INganSach_BLL
    {
        private readonly NganSach_DAL _dal;
        public NganSach_BLL(NganSach_DAL dal) => _dal = dal;

        public Task<List<NganSach>> GetBudgetsAsync(int taiKhoanId, bool includeDeleted = false)
            => _dal.GetByUserAsync(taiKhoanId, includeDeleted);

        public Task<int> CreateBudgetAsync(NganSachCreateRequest req)
            => _dal.CreateAsync(req);

        public Task<bool> UpdateBudgetAsync(int id, int taiKhoanId, NganSachUpdateRequest req)
            => _dal.UpdateAsync(id, taiKhoanId, req);

        public Task<bool> LockBudgetAsync(int id, int taiKhoanId, bool isLocked)
            => _dal.SetLockAsync(id, taiKhoanId, isLocked);

        public Task<bool> SoftDeleteBudgetAsync(int id, int taiKhoanId)
            => _dal.SoftDeleteAsync(id, taiKhoanId);
    }
}
