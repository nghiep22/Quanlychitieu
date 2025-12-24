using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class NganSach
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public int DanhMucId { get; set; }

        public decimal SoTienGioiHan { get; set; }

        public DateTime TGBatDau { get; set; }
        public DateTime TGKetThuc { get; set; }

        public string TrangThai { get; set; } = "Hoạt động"; // Hoạt động / Hết hạn / Khóa
        public string? GhiChu { get; set; }

        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    /* ===== DTO ===== */

    public class NganSachCreateRequest
    {
        public int TaiKhoanId { get; set; }
        public int DanhMucId { get; set; }
        public decimal SoTienGioiHan { get; set; }
        public DateTime TGBatDau { get; set; }
        public DateTime TGKetThuc { get; set; }
        public string? GhiChu { get; set; }
    }

    public class NganSachUpdateRequest : NganSachCreateRequest { }

    public class NganSachLockRequest
    {
        public int TaiKhoanId { get; set; }
        public bool IsLocked { get; set; }
    }

}
