using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class MucTieuTietKiem
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public string TenMucTieu { get; set; } = "";
        public decimal SoTienCanDat { get; set; }
        public decimal SoTienDaDat { get; set; }
        public string? MoTa { get; set; }
        public DateTime? HanHoanThanh { get; set; }
        public string TrangThai { get; set; } = "Đang thực hiện";
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    // DTO tạo mới
    public class MucTieuTietKiemCreateDto
    {
        public int TaiKhoanId { get; set; }
        public string TenMucTieu { get; set; } = "";
        public decimal SoTienCanDat { get; set; }
        public string? MoTa { get; set; }
        public DateTime? HanHoanThanh { get; set; }
    }

    // DTO cập nhật
    public class MucTieuTietKiemUpdateDto
    {
        public string TenMucTieu { get; set; } = "";
        public decimal SoTienCanDat { get; set; }
        public string? MoTa { get; set; }
        public DateTime? HanHoanThanh { get; set; }
        public string TrangThai { get; set; } = "Đang thực hiện";
    }

    // DTO nạp tiền vào mục tiêu
    public class MucTieuTietKiemNapDto
    {
        public decimal SoTienNap { get; set; }
    }
}



