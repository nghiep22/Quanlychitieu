using Models;

namespace BLL;

public interface IVi_BLL
{
    Task<List<Vi>> GetWalletsAsync(int taiKhoanId, bool includeDeleted = false);
    Task<Vi?> GetWalletAsync(int id, int taiKhoanId, bool includeDeleted = false);
    Task<int> CreateWalletAsync(ViCreateRequest req);
    Task<bool> UpdateWalletAsync(int id, int taiKhoanId, ViUpdateRequest req);
    Task<bool> LockWalletAsync(int id, int taiKhoanId, bool isLocked);
    Task<bool> SoftDeleteWalletAsync(int id, int taiKhoanId);
}
