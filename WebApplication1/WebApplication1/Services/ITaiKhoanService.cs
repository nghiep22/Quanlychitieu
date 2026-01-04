using QL_NganQuy.DTO;

namespace QL_NganQuy.Services
{
    public interface ITaiKhoanService
    {
        TaiKhoanDto GetById(int id);
        List<TaiKhoanDto> GetAll();
        List<TaiKhoanDto> Search(TaiKhoanFilterDto filter);

        TaiKhoanDto Create(CreateTaiKhoanDto dto);
        TaiKhoanDto Update(int id, UpdateTaiKhoanDto dto);
        bool Delete(int id);
    }
}