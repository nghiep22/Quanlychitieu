using DAL;
using Models;

namespace BLL
{
    public class DanhMuc_BLL : IDanhMuc_BLL
    {
        private readonly DanhMuc_DAL _dal;

        public DanhMuc_BLL(DanhMuc_DAL dal)
        {
            _dal = dal;
        }

        private static void ValidateLoai(string loai)
        {
            if (string.IsNullOrWhiteSpace(loai)) throw new ArgumentException("Loai is required (THU/CHI).");
            loai = loai.Trim().ToUpper();
            if (loai != "THU" && loai != "CHI") throw new ArgumentException("Loai must be THU or CHI.");
        }

        public List<DanhMuc> GetAll(int taiKhoanId, string? loai, string? status, bool includeDeleted)
        {
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");

            loai = string.IsNullOrWhiteSpace(loai) ? null : loai.Trim().ToUpper();
            if (loai != null) ValidateLoai(loai);

            status = string.IsNullOrWhiteSpace(status) ? null : status.Trim(); // "Hoạt động" | "Khóa"
            return _dal.GetAll(taiKhoanId, loai, status, includeDeleted);
        }

        public DanhMuc? GetById(int id, int taiKhoanId, bool includeDeleted)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");
            return _dal.GetById(id, taiKhoanId, includeDeleted);
        }

        public int Create(DanhMucCreateRequest req)
        {
            if (req.TaiKhoanId <= 0) throw new ArgumentException("TaiKhoanId must be > 0");
            if (string.IsNullOrWhiteSpace(req.TenDanhMuc)) throw new ArgumentException("TenDanhMuc is required");
            ValidateLoai(req.Loai);

            var ten = req.TenDanhMuc.Trim();
            var loai = req.Loai.Trim().ToUpper();

            if (_dal.ExistsTenLoai(req.TaiKhoanId, ten, loai))
                throw new InvalidOperationException("DUPLICATE_NAME_LOAI");

            var dm = new DanhMuc
            {
                TaiKhoanId = req.TaiKhoanId,
                TenDanhMuc = ten,
                Loai = loai,
                Icon = req.Icon,
                MauSac = req.MauSac,
                GhiChu = req.GhiChu
            };

            return _dal.Insert(dm);
        }

        public bool Update(int id, DanhMucUpdateRequest req)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (req.TaiKhoanId <= 0) throw new ArgumentException("TaiKhoanId must be > 0");
            if (string.IsNullOrWhiteSpace(req.TenDanhMuc)) throw new ArgumentException("TenDanhMuc is required");
            ValidateLoai(req.Loai);

            var ten = req.TenDanhMuc.Trim();
            var loai = req.Loai.Trim().ToUpper();

            if (_dal.ExistsTenLoai(req.TaiKhoanId, ten, loai, excludeId: id))
                throw new InvalidOperationException("DUPLICATE_NAME_LOAI");

            var dm = new DanhMuc
            {
                TaiKhoanId = req.TaiKhoanId,
                TenDanhMuc = ten,
                Loai = loai,
                Icon = req.Icon,
                MauSac = req.MauSac,
                GhiChu = req.GhiChu
            };

            return _dal.Update(id, dm);
        }

        public bool Lock(int id, DanhMucLockRequest req)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (req.TaiKhoanId <= 0) throw new ArgumentException("TaiKhoanId must be > 0");

            return _dal.SetLock(id, req.TaiKhoanId, req.IsLocked);
        }

        public bool Delete(int id, int taiKhoanId)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");

            return _dal.SoftDelete(id, taiKhoanId);
        }
    }
}
