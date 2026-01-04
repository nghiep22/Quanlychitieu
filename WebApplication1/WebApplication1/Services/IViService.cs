using QL_NganQuy.DTO;

namespace QL_NganQuy.Services
{
    public interface IViService
    {
        ViDto GetById(int id);
        List<ViDto> GetAll();
        List<ViDto> GetByTaiKhoanId(int taiKhoanId);
        List<ViDto> Search(ViFilterDto filter);

        ViDto Create(CreateViDto dto);
        ViDto Update(int id, UpdateViDto dto);
        bool Delete(int id);
    }
}