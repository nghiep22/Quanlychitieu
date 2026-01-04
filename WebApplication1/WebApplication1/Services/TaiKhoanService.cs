using QL_NganQuy.DTO;
using QL_NganQuy.Models;
using QL_NganQuy.Repositories;

namespace QL_NganQuy.Services
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private readonly ITaiKhoanRepository _repository;

        public TaiKhoanService(ITaiKhoanRepository repository)
        {
            _repository = repository;
        }

        public TaiKhoanDto GetById(int id)
        {
            var taiKhoan = _repository.GetById(id);
            if (taiKhoan == null)
                return null;

            return MapToDto(taiKhoan);
        }

        public List<TaiKhoanDto> GetAll()
        {
            var taiKhoans = _repository.GetAll();
            return taiKhoans.Select(MapToDto).ToList();
        }

        public List<TaiKhoanDto> Search(TaiKhoanFilterDto filter)
        {
            var taiKhoans = _repository.Search(filter.Quyen, filter.IsActive);
            return taiKhoans.Select(MapToDto).ToList();
        }

        public TaiKhoanDto Create(CreateTaiKhoanDto dto)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(dto.TenDangNhap))
                throw new ArgumentException("Tên đăng nhập không được để trống");

            if (string.IsNullOrWhiteSpace(dto.MatKhau))
                throw new ArgumentException("Mật khẩu không được để trống");

            // Kiểm tra tên đăng nhập đã tồn tại
            if (_repository.CheckTenDangNhapExists(dto.TenDangNhap, null))
                throw new ArgumentException("Tên đăng nhập đã tồn tại");

            // Tạo tài khoản mới
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = dto.TenDangNhap,
                MatKhau = dto.MatKhau,
                HoTen = dto.HoTen,
                Quyen = dto.Quyen ?? "User",
                IsActive = true
            };

            var id = _repository.Create(taiKhoan);
            taiKhoan.Id = id;

            return MapToDto(taiKhoan);
        }

        public TaiKhoanDto Update(int id, UpdateTaiKhoanDto dto)
        {
            var taiKhoan = _repository.GetById(id);
            if (taiKhoan == null)
                return null;

            taiKhoan.HoTen = dto.HoTen;
            taiKhoan.Quyen = dto.Quyen;
            taiKhoan.IsActive = dto.IsActive;

            _repository.Update(taiKhoan);

            return MapToDto(taiKhoan);
        }

        public bool Delete(int id)
        {
            var taiKhoan = _repository.GetById(id);
            if (taiKhoan == null)
                return false;

            return _repository.Delete(id);
        }

        private TaiKhoanDto MapToDto(TaiKhoan taiKhoan)
        {
            return new TaiKhoanDto
            {
                Id = taiKhoan.Id,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Quyen = taiKhoan.Quyen,
                IsActive = taiKhoan.IsActive
            };
        }
    }
}