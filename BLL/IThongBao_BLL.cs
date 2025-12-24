using Models;

namespace BLL
{
    public interface IThongBao_BLL
    {
        Task<List<ThongBao>> GetAllAsync(int taiKhoanId, string? loai, string? trangThai, DateTime? from, DateTime? to);
        Task<ThongBao?> GetByIdAsync(int id, int taiKhoanId);
        Task<int> CreateAsync(ThongBaoCreateRequest req);
        Task<bool> UpdateTrangThaiAsync(int id, int taiKhoanId, bool daXem);
        Task<bool> DeleteAsync(int id, int taiKhoanId);
    }
}
