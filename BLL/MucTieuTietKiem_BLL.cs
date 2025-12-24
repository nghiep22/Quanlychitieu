using DAL;
using Models;

namespace BLL
{
    public class MucTieuTietKiem_BLL : IMucTieuTietKiem_BLL
    {
        private readonly MucTieuTietKiem_DAL _dal;

        public MucTieuTietKiem_BLL(MucTieuTietKiem_DAL dal)
        {
            _dal = dal;
        }

        public List<MucTieuTietKiem> GetAll(int taiKhoanId, string? trangThai, bool includeDeleted)
        {
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId không hợp lệ.");
            if (trangThai != null && trangThai != "Đang thực hiện" && trangThai != "Hoàn thành" && trangThai != "Hủy")
                throw new ArgumentException("trangThai không hợp lệ (Đang thực hiện/Hoàn thành/Hủy).");

            return _dal.GetAll(taiKhoanId, trangThai, includeDeleted);
        }

        public MucTieuTietKiem GetById(int id, int taiKhoanId)
        {
            if (id <= 0) throw new ArgumentException("id không hợp lệ.");
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId không hợp lệ.");

            var item = _dal.GetById(id, taiKhoanId);
            if (item == null) throw new KeyNotFoundException("Không tìm thấy mục tiêu.");
            return item;
        }

        public int Create(MucTieuTietKiemCreateDto dto)
        {
            if (dto.TaiKhoanId <= 0) throw new ArgumentException("TaiKhoanId không hợp lệ.");
            if (string.IsNullOrWhiteSpace(dto.TenMucTieu)) throw new ArgumentException("Tên mục tiêu không được trống.");
            if (dto.SoTienCanDat <= 0) throw new ArgumentException("Số tiền cần đạt phải > 0.");

            return _dal.Create(dto);
        }

        public void Update(int id, int taiKhoanId, MucTieuTietKiemUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TenMucTieu)) throw new ArgumentException("Tên mục tiêu không được trống.");
            if (dto.SoTienCanDat <= 0) throw new ArgumentException("Số tiền cần đạt phải > 0.");
            if (dto.TrangThai != "Đang thực hiện" && dto.TrangThai != "Hoàn thành" && dto.TrangThai != "Hủy")
                throw new ArgumentException("Trạng thái không hợp lệ.");

            var ok = _dal.Update(id, taiKhoanId, dto);
            if (!ok) throw new KeyNotFoundException("Không tìm thấy mục tiêu để cập nhật.");
        }

        public void AddMoney(int id, int taiKhoanId, decimal soTienNap)
        {
            if (soTienNap <= 0) throw new ArgumentException("Số tiền nạp phải > 0.");

            var ok = _dal.AddMoney(id, taiKhoanId, soTienNap);
            if (!ok) throw new KeyNotFoundException("Không tìm thấy mục tiêu để nạp tiền (hoặc đã xóa).");
        }

        public void SoftDelete(int id, int taiKhoanId)
        {
            var ok = _dal.SoftDelete(id, taiKhoanId);
            if (!ok) throw new KeyNotFoundException("Không tìm thấy mục tiêu để xóa.");
        }

        public void Restore(int id, int taiKhoanId)
        {
            var ok = _dal.Restore(id, taiKhoanId);
            if (!ok) throw new KeyNotFoundException("Không tìm thấy mục tiêu để khôi phục.");
        }
    }
}
