using Models;

namespace BLL
{
    public interface IWalletTransfer_BLL
    {
        ChuyenTienVi Create(WalletTransferCreateRequest req);
        List<ChuyenTienVi> GetList(WalletTransferQuery q);
        ChuyenTienVi? GetById(int id, int taiKhoanId);
        bool SoftDelete(int id, int taiKhoanId);
    }
}
