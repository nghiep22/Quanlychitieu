using QL_NganQuy.Models;

namespace QL_NganQuy.Repositories
{
    public interface ITaiKhoanRepository
    {
        TaiKhoan GetById(int id);
        List<TaiKhoan> GetAll();
        List<TaiKhoan> Search(string quyen, bool? isActive);

        int Create(TaiKhoan taiKhoan);
        bool Update(TaiKhoan taiKhoan);
        bool Delete(int id);

        bool CheckTenDangNhapExists(string tenDangNhap, int? excludeId);
    }
}