namespace Models
{
    public class WalletTransferQuery
    {
        public int TaiKhoanId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? ViNguonId { get; set; }
        public int? ViDichId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeDeleted { get; set; } = false;
    }
}