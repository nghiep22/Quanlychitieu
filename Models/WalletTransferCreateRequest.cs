namespace Models
{
    public class WalletTransferCreateRequest
    {
        public int TaiKhoanId { get; set; }
        public int ViNguonId { get; set; }
        public int ViDichId { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayChuyen { get; set; } // yyyy-MM-dd
        public string? GhiChu { get; set; }
    }
}
