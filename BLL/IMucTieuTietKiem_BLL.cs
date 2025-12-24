using Models;

namespace BLL
{
    public interface IMucTieuTietKiem_BLL
    {
        List<MucTieuTietKiem> GetAll(int taiKhoanId, string? trangThai, bool includeDeleted);
        MucTieuTietKiem GetById(int id, int taiKhoanId);
        int Create(MucTieuTietKiemCreateDto dto);
        void Update(int id, int taiKhoanId, MucTieuTietKiemUpdateDto dto);
        void AddMoney(int id, int taiKhoanId, decimal soTienNap);
        void SoftDelete(int id, int taiKhoanId);
        void Restore(int id, int taiKhoanId);
    }
}
