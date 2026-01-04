using QL_NganQuy.Models;

namespace QL_NganQuy.Repositories
{
    public interface IViRepository
    {
        Vi GetById(int id);
        List<Vi> GetAll();
        List<Vi> GetByTaiKhoanId(int taiKhoanId);
        List<Vi> Search(int taiKhoanId, string loaiVi, string trangThai);

        int Create(Vi vi);
        bool Update(Vi vi);
        bool Delete(int id);

        bool CheckTenViExists(int taiKhoanId, string tenVi, int? excludeId);
    }
}