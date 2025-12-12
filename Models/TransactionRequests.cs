namespace Models
{
    public class TransactionCreateRequest
    {
        public int TaiKhoanId { get; set; }
        public int ViId { get; set; }
        public int DanhMucId { get; set; }
        public decimal SoTien { get; set; }
        public string LoaiGD { get; set; } = "";  // THU/CHI
        public DateTime NgayGD { get; set; }      // yyyy-MM-dd
        public string? GhiChu { get; set; }
        public string? AnhHoaDon { get; set; }
    }

    public class TransactionUpdateRequest : TransactionCreateRequest { }
}
