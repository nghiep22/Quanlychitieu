using DAL;
using Models;

namespace BLL
{
    public class TaiKhoan_BLL : ITaiKhoan_BLL
    {
        private readonly TaiKhoan_DAL _taiKhoanDal;

        public TaiKhoan_BLL(TaiKhoan_DAL taiKhoanDal)
        {
            _taiKhoanDal = taiKhoanDal;
        }

        public async Task<TaiKhoan?> DangNhapAsync(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
                return null;

            var user = await _taiKhoanDal.GetByTenDangNhapAsync(tenDangNhap);
            if (user == null) return null;

            // DEMO: so sánh mật khẩu thường
            if (user.MatKhau != matKhau)
                return null;

            return user;
        }
    }
}
