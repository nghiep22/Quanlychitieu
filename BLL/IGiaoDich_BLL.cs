using Models;

namespace BLL
{
    public interface IGiaoDich_BLL
    {
        List<GiaoDich> Query(int taiKhoanId, DateTime? from, DateTime? to, int? viId, int? danhMucId,
                             string? loai, string? q, int page, int pageSize, string sort, bool includeDeleted);

        GiaoDich? GetById(int id, int taiKhoanId, bool includeDeleted);

        int Create(TransactionCreateRequest req);
        bool Update(int id, TransactionUpdateRequest req);

        bool Delete(int id, int taiKhoanId);
        bool Restore(int id, int taiKhoanId);
    }
}
