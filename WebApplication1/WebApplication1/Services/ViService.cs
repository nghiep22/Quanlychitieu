using QL_NganQuy.DTO;
using QL_NganQuy.Models;
using QL_NganQuy.Repositories;

namespace QL_NganQuy.Services
{
    public class ViService : IViService
    {
        private readonly IViRepository _repository;

        public ViService(IViRepository repository)
        {
            _repository = repository;
        }

        public ViDto GetById(int id)
        {
            var vi = _repository.GetById(id);
            if (vi == null)
                return null;

            return MapToDto(vi);
        }

        public List<ViDto> GetAll()
        {
            var vis = _repository.GetAll();
            return vis.Select(MapToDto).ToList();
        }

        public List<ViDto> GetByTaiKhoanId(int taiKhoanId)
        {
            var vis = _repository.GetByTaiKhoanId(taiKhoanId);
            return vis.Select(MapToDto).ToList();
        }

        public List<ViDto> Search(ViFilterDto filter)
        {
            var vis = _repository.Search(filter.TaiKhoanId, filter.LoaiVi, filter.TrangThai);
            return vis.Select(MapToDto).ToList();
        }

        public ViDto Create(CreateViDto dto)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(dto.TenVi))
                throw new ArgumentException("Tên ví không được để trống");

            if (string.IsNullOrWhiteSpace(dto.LoaiVi))
                throw new ArgumentException("Loại ví không được để trống");

            if (!IsValidLoaiVi(dto.LoaiVi))
                throw new ArgumentException("Loại ví không hợp lệ");

            if (dto.SoDuBanDau < 0)
                throw new ArgumentException("Số dư ban đầu không được âm");

            // Kiểm tra tên ví đã tồn tại
            if (_repository.CheckTenViExists(dto.TaiKhoanId, dto.TenVi, null))
                throw new ArgumentException("Tên ví đã tồn tại");

            // Tạo ví mới
            var vi = new Vi
            {
                TaiKhoanId = dto.TaiKhoanId,
                TenVi = dto.TenVi,
                LoaiVi = dto.LoaiVi,
                SoDuBanDau = dto.SoDuBanDau,
                GhiChu = dto.GhiChu,
                TrangThai = "Hoạt động",
                DaXoa = false,
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now
            };

            var id = _repository.Create(vi);
            vi.Id = id;

            return MapToDto(vi);
        }

        public ViDto Update(int id, UpdateViDto dto)
        {
            var vi = _repository.GetById(id);
            if (vi == null)
                return null;

            // Validate chỉ khi có truyền giá trị
            if (!string.IsNullOrWhiteSpace(dto.LoaiVi))
            {
                if (!IsValidLoaiVi(dto.LoaiVi))
                    throw new ArgumentException("Loại ví không hợp lệ");
                vi.LoaiVi = dto.LoaiVi;
            }

            if (!string.IsNullOrWhiteSpace(dto.TrangThai))
            {
                if (!IsValidTrangThai(dto.TrangThai))
                    throw new ArgumentException("Trạng thái không hợp lệ");
                vi.TrangThai = dto.TrangThai;
            }

            // Cập nhật tên ví nếu có
            if (!string.IsNullOrWhiteSpace(dto.TenVi))
            {
                if (dto.TenVi != vi.TenVi)
                {
                    if (_repository.CheckTenViExists(vi.TaiKhoanId, dto.TenVi, id))
                        throw new ArgumentException("Tên ví đã tồn tại");
                }
                vi.TenVi = dto.TenVi;
            }

            // Cập nhật ghi chú nếu có (cho phép ghi chú rỗng)
            if (dto.GhiChu != null)
                vi.GhiChu = dto.GhiChu;

            vi.NgayCapNhat = DateTime.Now;

            _repository.Update(vi);

            return MapToDto(vi);
        }

        public bool Delete(int id)
        {
            var vi = _repository.GetById(id);
            if (vi == null)
                return false;

            return _repository.Delete(id);
        }

        private ViDto MapToDto(Vi vi)
        {
            return new ViDto
            {
                Id = vi.Id,
                TaiKhoanId = vi.TaiKhoanId,
                TenVi = vi.TenVi,
                LoaiVi = vi.LoaiVi,
                SoDuBanDau = vi.SoDuBanDau,
                GhiChu = vi.GhiChu,
                TrangThai = vi.TrangThai,
                NgayTao = vi.NgayTao,
                NgayCapNhat = vi.NgayCapNhat
            };
        }

        private bool IsValidLoaiVi(string loaiVi)
        {
            var validLoai = new[] { "Tiền mặt", "Ngân hàng", "Ví điện tử" };
            return validLoai.Contains(loaiVi);
        }

        private bool IsValidTrangThai(string trangThai)
        {
            var validTrangThai = new[] { "Hoạt động", "Khóa" };
            return validTrangThai.Contains(trangThai);
        }
    }
}