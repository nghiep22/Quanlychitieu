using DAL;
using Models;

namespace BLL
{
    public class WalletTransfer_BLL : IWalletTransfer_BLL
    {
        private readonly WalletTransfer_DAL _dal;

        public WalletTransfer_BLL(WalletTransfer_DAL dal)
        {
            _dal = dal;
        }

        public ChuyenTienVi Create(WalletTransferCreateRequest req) => _dal.CreateTransfer(req);

        public List<ChuyenTienVi> GetList(WalletTransferQuery q) => _dal.GetList(q);

        public ChuyenTienVi? GetById(int id, int taiKhoanId) => _dal.GetById(id, taiKhoanId);

        public bool SoftDelete(int id, int taiKhoanId) => _dal.SoftDelete(id, taiKhoanId);
    }
}
