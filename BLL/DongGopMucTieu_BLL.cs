using DAL;
using Models;

namespace BLL
{
    public class DongGopMucTieu_BLL : IDongGopMucTieu_BLL
    {
        private readonly DongGopMucTieu_DAL _dal;
        public DongGopMucTieu_BLL(DongGopMucTieu_DAL dal) => _dal = dal;

        public async Task<int> CreateAsync(DongGopMucTieuCreateRequest req)
        {
            if (req == null) throw new ArgumentException("Dữ liệu yêu cầu không được để trống");
            if (req.MucTieuId <= 0) throw new ArgumentException("Mục tiêu không hợp lệ");
            if (req.TaiKhoanId <= 0) throw new ArgumentException("Tài khoản không hợp lệ");
            if (req.SoTien <= 0) throw new ArgumentException("Số tiền đóng góp phải lớn hơn 0");

            // Mặc định ngày đóng góp nếu không có
            if (req.NgayDongGop == default) req.NgayDongGop = DateTime.Now;

            return await _dal.CreateAsync(req);
        }

        public Task<DongGopMucTieu?> GetByIdAsync(int id, int taiKhoanId, bool includeDeleted)
        {
            if (id <= 0 || taiKhoanId <= 0) throw new ArgumentException("Tham số không hợp lệ");
            return _dal.GetByIdAsync(id, taiKhoanId, includeDeleted);
        }

        public Task<List<DongGopMucTieu>> GetByMucTieuAsync(int mucTieuId, bool includeDeleted)
        {
            if (mucTieuId <= 0) throw new ArgumentException("ID mục tiêu không hợp lệ");
            return _dal.GetByMucTieuAsync(mucTieuId, includeDeleted);
        }

        public Task<bool> UpdateAsync(int id, int taiKhoanId, DongGopMucTieuUpdateRequest req)
        {
            if (id <= 0 || req.SoTien <= 0) throw new ArgumentException("Dữ liệu cập nhật không hợp lệ");
            return _dal.UpdateAsync(id, taiKhoanId, req);
        }

        public Task<bool> SoftDeleteAsync(int id, int taiKhoanId)
        {
            return _dal.SoftDeleteAsync(id, taiKhoanId);
        }
    }
}