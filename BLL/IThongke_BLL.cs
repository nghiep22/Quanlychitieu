using Models.Reports;

namespace BLL
{
    public interface IReports_BLL
    {
        ReportSummaryDto Summary(int taiKhoanId, int month, int year, int? viId);
        List<CategoryReportDto> ByCategory(int taiKhoanId, int month, int year, int? viId);
        List<WalletReportDto> ByWallet(int taiKhoanId, int month, int year);
        List<CashflowPointDto> Cashflow(int taiKhoanId, DateTime from, DateTime to, string groupBy, int? viId);
    }
}
