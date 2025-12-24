using DAL;
using Models;

namespace BLL
{
    public class DongGopMucTieu_BLL : IDongGopMucTieu_BLL
    {
        private readonly DongGopMucTieu_DAL _dal;

        public DongGopMucTieu_BLL(DongGopMucTieu_DAL dal)
        {
            _dal = dal;
        }

        private static void ValidateIds(int mucTieuId, int taiKhoanId, int giaoDichId)
        {
            if (mucTieuId <= 0) throw new ArgumentException("MucTieuId must be > 0");
            if (taiKhoanId <= 0) throw new ArgumentException("TaiKhoanId must be > 0");
            if (giaoDichId <= 0) throw new ArgumentException("GiaoDichId must be > 0");
        }

        public Task<List<DongGopMucTieu>> GetByMucTieuAsync(int mucTieuId, bool includeDeleted)
        {
            if (mucTieuId <= 0) throw new ArgumentException("mucTieuId must be > 0");
            return _dal.GetByMucTieuAsync(mucTieuId, includeDeleted);
        }

        public Task<DongGopMucTieu?> GetByIdAsync(int id, int taiKhoanId, bool includeDeleted)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");
            return _dal.GetByIdAsync(id, taiKhoanId, includeDeleted);
        }

        public Task<int> CreateAsync(DongGopMucTieuCreateRequest req)
        {
            if (req == null) throw new ArgumentException("Body is required");
            ValidateIds(req.MucTieuId, req.TaiKhoanId, req.GiaoDichId);

            if (req.SoTien <= 0) throw new ArgumentException("SoTien must be > 0");
            if (req.NgayDongGop == default) throw new ArgumentException("NgayDongGop is required");

            return _dal.CreateAsync(req);
        }

        public Task<bool> UpdateAsync(int id, int taiKhoanId, DongGopMucTieuUpdateRequest req)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");
            if (req == null) throw new ArgumentException("Body is required");
            if (req.SoTien <= 0) throw new ArgumentException("SoTien must be > 0");
            if (req.NgayDongGop == default) throw new ArgumentException("NgayDongGop is required");

            return _dal.UpdateAsync(id, taiKhoanId, req);
        }

        public Task<bool> SoftDeleteAsync(int id, int taiKhoanId)
        {
            if (id <= 0) throw new ArgumentException("id must be > 0");
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");
            return _dal.SoftDeleteAsync(id, taiKhoanId);
        }
    }
}
