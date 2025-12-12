using DAL;
using Microsoft.Data.SqlClient;
using Models;

namespace BLL;

public class Vi_BLL : IVi_BLL
{
    private readonly Vi_DAL _dal;
    public Vi_BLL(Vi_DAL dal) => _dal = dal;

    public Task<List<Vi>> GetWalletsAsync(int taiKhoanId, bool includeDeleted = false)
        => _dal.GetByUserAsync(taiKhoanId, includeDeleted);

    public Task<Vi?> GetWalletAsync(int id, int taiKhoanId, bool includeDeleted = false)
        => _dal.GetByIdAsync(id, taiKhoanId, includeDeleted);

    public Task<int> CreateWalletAsync(ViCreateRequest req)
        => _dal.CreateAsync(req);

    public Task<bool> UpdateWalletAsync(int id, int taiKhoanId, ViUpdateRequest req)
        => _dal.UpdateAsync(id, taiKhoanId, req);

    public Task<bool> LockWalletAsync(int id, int taiKhoanId, bool isLocked)
        => _dal.SetLockAsync(id, taiKhoanId, isLocked);

    public Task<bool> SoftDeleteWalletAsync(int id, int taiKhoanId)
        => _dal.SoftDeleteAsync(id, taiKhoanId);
}
