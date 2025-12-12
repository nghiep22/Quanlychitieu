namespace Models;

public class Vi
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public string TenVi { get; set; } = string.Empty;
    public string LoaiVi { get; set; } = string.Empty;   // 'Tiền mặt'/'Ngân hàng'/'Ví điện tử'
    public decimal SoDuBanDau { get; set; }
    public string? GhiChu { get; set; }
    public string TrangThai { get; set; } = "Hoạt động"; // 'Hoạt động'/'Khóa'
    public bool DaXoa { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

// DTO tạo/sửa
public class ViCreateRequest
{
    public int TaiKhoanId { get; set; }
    public string TenVi { get; set; } = string.Empty;
    public string LoaiVi { get; set; } = string.Empty;
    public decimal SoDuBanDau { get; set; }
    public string? GhiChu { get; set; }
}

public class ViUpdateRequest : ViCreateRequest { }

// DTO khóa/mở
public class WalletLockRequest
{
    public int TaiKhoanId { get; set; }
    public bool IsLocked { get; set; }  // true=Khóa, false=Mở
}
