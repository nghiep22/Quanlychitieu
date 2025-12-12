using DAL;
using Models;

namespace BLL
{
    public class GiaoDich_BLL : IGiaoDich_BLL
    {
        private readonly GiaoDich_DAL _dal;
        public GiaoDich_BLL(GiaoDich_DAL dal) => _dal = dal;

        private static string NormLoai(string? loai)
            => (loai ?? "").Trim().ToUpper();

        private void ValidateOwnershipAndLogic(int taiKhoanId, int viId, int danhMucId, string loaiGd)
        {
            if (!_dal.WalletBelongsToUser(taiKhoanId, viId))
                throw new InvalidOperationException("WALLET_NOT_BELONG_USER");

            var dmLoai = _dal.GetDanhMucLoaiIfBelongsToUser(taiKhoanId, danhMucId);
            if (dmLoai == null)
                throw new InvalidOperationException("CATEGORY_NOT_BELONG_USER");

            if (NormLoai(dmLoai) != NormLoai(loaiGd))
                throw new InvalidOperationException("LOAIGD_NOT_MATCH_CATEGORY");
        }

        public List<GiaoDich> Query(int taiKhoanId, DateTime? from, DateTime? to, int? viId, int? danhMucId,
                                    string? loai, string? q, int page, int pageSize, string sort, bool includeDeleted)
        {
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            loai = string.IsNullOrWhiteSpace(loai) ? null : NormLoai(loai);
            if (loai != null && loai != "THU" && loai != "CHI")
                throw new ArgumentException("loai must be THU or CHI");

            return _dal.Query(taiKhoanId, from, to, viId, danhMucId, loai, q, page, pageSize, sort, includeDeleted);
        }

        public GiaoDich? GetById(int id, int taiKhoanId, bool includeDeleted)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");
            return _dal.GetById(id, taiKhoanId, includeDeleted);
        }

        public int Create(TransactionCreateRequest req)
        {
            if (req.TaiKhoanId <= 0) throw new ArgumentException("TaiKhoanId must be > 0");
            if (req.SoTien <= 0) throw new ArgumentException("SoTien must be > 0");

            req.LoaiGD = NormLoai(req.LoaiGD);
            if (req.LoaiGD != "THU" && req.LoaiGD != "CHI")
                throw new ArgumentException("LoaiGD must be THU or CHI");

            ValidateOwnershipAndLogic(req.TaiKhoanId, req.ViId, req.DanhMucId, req.LoaiGD);
            return _dal.Insert(req);
        }

        public bool Update(int id, TransactionUpdateRequest req)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (req.TaiKhoanId <= 0) throw new ArgumentException("TaiKhoanId must be > 0");
            if (req.SoTien <= 0) throw new ArgumentException("SoTien must be > 0");

            req.LoaiGD = NormLoai(req.LoaiGD);
            if (req.LoaiGD != "THU" && req.LoaiGD != "CHI")
                throw new ArgumentException("LoaiGD must be THU or CHI");

            // rule: đổi gì cũng kiểm tra lại toàn bộ
            ValidateOwnershipAndLogic(req.TaiKhoanId, req.ViId, req.DanhMucId, req.LoaiGD);

            return _dal.Update(id, req);
        }

        public bool Delete(int id, int taiKhoanId)
            => _dal.SoftDelete(id, taiKhoanId);

        public bool Restore(int id, int taiKhoanId)
            => _dal.Restore(id, taiKhoanId);
    }
}
