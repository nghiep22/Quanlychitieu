using Models;

namespace BLL
{
    public interface ITaiKhoan_BLL
    {
        Task<TaiKhoan?> DangNhapAsync(string tenDangNhap, string matKhau);
    }
}
