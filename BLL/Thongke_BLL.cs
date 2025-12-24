using DAL;
using Models.Reports;

namespace BLL
{
    public class Reports_BLL : IReports_BLL
    {
        private readonly Reports_DAL _dal;

        public Reports_BLL(Reports_DAL dal)
        {
            _dal = dal;
        }

        private static void ValidateMonthYear(int month, int year)
        {
            if (month < 1 || month > 12) throw new ArgumentException("month must be 1..12");
            if (year < 2000 || year > 2100) throw new ArgumentException("year is invalid");
        }

        private static void ValidateTaiKhoan(int taiKhoanId)
        {
            if (taiKhoanId <= 0) throw new ArgumentException("taiKhoanId must be > 0");
        }

        public ReportSummaryDto Summary(int taiKhoanId, int month, int year, int? viId)
        {
            ValidateTaiKhoan(taiKhoanId);
            ValidateMonthYear(month, year);
            return _dal.GetSummary(taiKhoanId, month, year, viId);
        }

        public List<CategoryReportDto> ByCategory(int taiKhoanId, int month, int year, int? viId)
        {
            ValidateTaiKhoan(taiKhoanId);
            ValidateMonthYear(month, year);
            return _dal.GetByCategory(taiKhoanId, month, year, viId);
        }

        public List<WalletReportDto> ByWallet(int taiKhoanId, int month, int year)
        {
            ValidateTaiKhoan(taiKhoanId);
            ValidateMonthYear(month, year);
            return _dal.GetByWallet(taiKhoanId, month, year);
        }

        public List<CashflowPointDto> Cashflow(int taiKhoanId, DateTime from, DateTime to, string groupBy, int? viId)
        {
            ValidateTaiKhoan(taiKhoanId);

            if (from.Date > to.Date) throw new ArgumentException("from must be <= to");
            groupBy = (groupBy ?? "day").Trim().ToLower();
            if (groupBy != "day" && groupBy != "week")
                throw new ArgumentException("groupBy must be 'day' or 'week'");

            return _dal.GetCashflow(taiKhoanId, from, to, groupBy, viId);
        }
    }
}
