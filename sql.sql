create database QL_NganQuy;
go
USE QL_NganQuy;
GO



------------------------------------------------------------
-- 1) TAIKHOAN
------------------------------------------------------------
CREATE TABLE dbo.TaiKhoan
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap NVARCHAR(100) NOT NULL UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,     -- lưu HASH từ API (khuyến nghị)
    HoTen NVARCHAR(200) NULL,
    Quyen NVARCHAR(50) NULL,            -- 'Admin' / 'User'
    IsActive BIT NOT NULL DEFAULT 1
);
go
CREATE INDEX IX_TaiKhoan_TenDangNhap ON dbo.TaiKhoan(TenDangNhap);
go
------------------------------------------------------------
-- 2) VI
------------------------------------------------------------
CREATE TABLE dbo.Vi
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId INT NOT NULL,
    TenVi NVARCHAR(100) NOT NULL,
    LoaiVi NVARCHAR(50) NOT NULL,
    SoDuBanDau DECIMAL(18,2) NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(500) NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Hoạt động',
    DaXoa BIT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Vi_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT CK_Vi_SoDu CHECK (SoDuBanDau >= 0),
    CONSTRAINT CK_Vi_Loai CHECK (LoaiVi IN (N'Tiền mặt', N'Ngân hàng', N'Ví điện tử')),
    CONSTRAINT CK_Vi_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Khóa'))
);
go
-- để GiaoDich FK (ViId, TaiKhoanId) đảm bảo ví thuộc đúng user
ALTER TABLE dbo.Vi
ADD CONSTRAINT UQ_Vi_Id_User UNIQUE (Id, TaiKhoanId);
go
CREATE INDEX IX_Vi_TaiKhoan ON dbo.Vi(TaiKhoanId);
go
CREATE UNIQUE INDEX UX_Vi_User_TenVi_NotDeleted
ON dbo.Vi(TaiKhoanId, TenVi)
WHERE DaXoa = 0;
go
------------------------------------------------------------
-- 3) DANHMUC
------------------------------------------------------------
CREATE TABLE dbo.DanhMuc
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId INT NOT NULL,
    TenDanhMuc NVARCHAR(200) NOT NULL,
    Loai CHAR(3) NOT NULL,               -- THU/CHI
    Icon NVARCHAR(100) NULL,
    MauSac NVARCHAR(20) NULL,
    GhiChu NVARCHAR(500) NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Hoạt động',
    DaXoa BIT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_DanhMuc_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT CK_DanhMuc_Loai CHECK (Loai IN ('THU','CHI')),
    CONSTRAINT CK_DanhMuc_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Khóa'))
);
go
-- để GiaoDich/NganSach FK (DanhMucId, TaiKhoanId) đảm bảo danh mục thuộc đúng user
ALTER TABLE dbo.DanhMuc
ADD CONSTRAINT UQ_DanhMuc_Id_User UNIQUE (Id, TaiKhoanId);
go
CREATE UNIQUE INDEX UX_DanhMuc_User_Ten_Loai_NotDeleted
ON dbo.DanhMuc(TaiKhoanId, TenDanhMuc, Loai)
WHERE DaXoa = 0;
go 

CREATE INDEX IX_DanhMuc_User_Loai ON dbo.DanhMuc(TaiKhoanId, Loai);
go
------------------------------------------------------------
-- 4) GIAODICH
------------------------------------------------------------
CREATE TABLE dbo.GiaoDich
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId INT NOT NULL,
    ViId INT NOT NULL,
    DanhMucId INT NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    LoaiGD CHAR(3) NOT NULL,             -- THU/CHI
    NgayGD DATE NOT NULL,
    GhiChu NVARCHAR(1000) NULL,
    AnhHoaDon NVARCHAR(260) NULL,
    DaXoa BIT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT CK_GiaoDich_SoTien CHECK (SoTien > 0),
    CONSTRAINT CK_GiaoDich_LoaiGD CHECK (LoaiGD IN ('THU','CHI')),

    CONSTRAINT FK_GD_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_GD_Vi_User FOREIGN KEY (ViId, TaiKhoanId) REFERENCES dbo.Vi(Id, TaiKhoanId),
    CONSTRAINT FK_GD_DanhMuc_User FOREIGN KEY (DanhMucId, TaiKhoanId) REFERENCES dbo.DanhMuc(Id, TaiKhoanId)
);
go
CREATE INDEX IX_GD_User_Ngay ON dbo.GiaoDich(TaiKhoanId, NgayGD DESC);
go
CREATE INDEX IX_GD_Vi_Ngay ON dbo.GiaoDich(ViId, NgayGD DESC);
go
CREATE INDEX IX_GD_DanhMuc_Ngay ON dbo.GiaoDich(DanhMucId, NgayGD DESC);
go

------------------------------------------------------------
-- 5) NGANSACH
------------------------------------------------------------
CREATE TABLE dbo.NganSach
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId INT NOT NULL,
    DanhMucId INT NOT NULL,
    SoTienGioiHan DECIMAL(18,2) NOT NULL,
    TGBatDau DATE NOT NULL,
    TGKetThuc DATE NOT NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Hoạt động',
    GhiChu NVARCHAR(500) NULL,
    DaXoa BIT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_NS_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_NS_DanhMuc_User FOREIGN KEY (DanhMucId, TaiKhoanId) REFERENCES dbo.DanhMuc(Id, TaiKhoanId),

    CONSTRAINT CK_NS_SoTien CHECK (SoTienGioiHan > 0),
    CONSTRAINT CK_NS_Date CHECK (TGKetThuc >= TGBatDau),
    CONSTRAINT CK_NS_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Hết hạn', N'Khóa'))
);
go
CREATE INDEX IX_NS_User_Time ON dbo.NganSach(TaiKhoanId, TGBatDau DESC, TGKetThuc);
go
------------------------------------------------------------
-- 6) MUCTIEUTIETKIEM
------------------------------------------------------------
CREATE TABLE dbo.MucTieuTietKiem
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId INT NOT NULL,
    TenMucTieu NVARCHAR(200) NOT NULL,
    SoTienCanDat DECIMAL(18,2) NOT NULL,
    SoTienDaDat DECIMAL(18,2) NOT NULL DEFAULT 0,
    MoTa NVARCHAR(500) NULL,
    HanHoanThanh DATE NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Đang thực hiện',
    DaXoa BIT NOT NULL DEFAULT 0,
NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_MT_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT CK_MT_CanDat CHECK (SoTienCanDat > 0),
    CONSTRAINT CK_MT_DaDat CHECK (SoTienDaDat >= 0),
    CONSTRAINT CK_MT_TrangThai CHECK (TrangThai IN (N'Đang thực hiện', N'Hoàn thành', N'Hủy'))
);
go
CREATE INDEX IX_MT_User_TrangThai ON dbo.MucTieuTietKiem(TaiKhoanId, TrangThai);
go
------------------------------------------------------------
-- 7) THANHTOAN (tùy chọn)
------------------------------------------------------------
CREATE TABLE dbo.ThanhToan
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    GiaoDichId INT NOT NULL UNIQUE,
    PhuongThuc NVARCHAR(50) NOT NULL,
    SoTienThanhToan DECIMAL(18,2) NOT NULL,
    NgayThanhToan DATE NOT NULL DEFAULT CAST(SYSDATETIME() AS DATE),
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Đã thanh toán',
    GhiChu NVARCHAR(500) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_TT_GiaoDich FOREIGN KEY (GiaoDichId) REFERENCES dbo.GiaoDich(Id),
    CONSTRAINT CK_TT_SoTien CHECK (SoTienThanhToan > 0),
    CONSTRAINT CK_TT_TrangThai CHECK (TrangThai IN (N'Đã thanh toán', N'Chờ thanh toán'))
);
go
CREATE INDEX IX_TT_Ngay ON dbo.ThanhToan(NgayThanhToan DESC);
go
------------------------------------------------------------
-- 8) THONGBAO
------------------------------------------------------------
CREATE TABLE dbo.ThongBao
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId INT NOT NULL,
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDung NVARCHAR(1000) NOT NULL,
    Loai NVARCHAR(50) NOT NULL,
    GiaoDichId INT NULL,
    NganSachId INT NULL,
    MucTieuId INT NULL,
    ThoiGianGui DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'Chưa xem',

    CONSTRAINT FK_TB_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_TB_GiaoDich FOREIGN KEY (GiaoDichId) REFERENCES dbo.GiaoDich(Id),
    CONSTRAINT FK_TB_NganSach FOREIGN KEY (NganSachId) REFERENCES dbo.NganSach(Id),
    CONSTRAINT FK_TB_MucTieu FOREIGN KEY (MucTieuId) REFERENCES dbo.MucTieuTietKiem(Id),

    CONSTRAINT CK_TB_Loai CHECK (Loai IN (N'Cảnh báo', N'Thông tin', N'Thành công', N'Lỗi')),
    CONSTRAINT CK_TB_TrangThai CHECK (TrangThai IN (N'Chưa xem', N'Đã xem'))
);
go
CREATE INDEX IX_TB_User_Time ON dbo.ThongBao(TaiKhoanId, ThoiGianGui DESC);
go
------------------------------------------------------------
-- 9) NHATKY_HETHONG
------------------------------------------------------------
CREATE TABLE dbo.NhatKyHeThong
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId INT NULL,
    DoiTuong NVARCHAR(50) NOT NULL,
    MaDoiTuong NVARCHAR(50) NOT NULL,
    HanhDong NVARCHAR(50) NOT NULL,
    NoiDung NVARCHAR(1000) NULL,
    GiaTriCu NVARCHAR(1000) NULL,
GiaTriMoi NVARCHAR(1000) NULL,
    DiaChiIP NVARCHAR(50) NULL,
    ThoiGian DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_NK_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT CK_NK_HanhDong CHECK (HanhDong IN (N'Tạo', N'Chỉnh sửa', N'Xóa', N'Khôi phục', N'Đăng nhập', N'Đăng xuất'))
);
go
CREATE INDEX IX_NK_Time ON dbo.NhatKyHeThong(ThoiGian DESC);
go
------------------------------------------------------------
-- 10) CHUYENTIEN_VI
------------------------------------------------------------
CREATE TABLE dbo.ChuyenTienVi
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanId INT NOT NULL,
    ViNguonId INT NOT NULL,
    ViDichId INT NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    NgayChuyen DATE NOT NULL,
    GhiChu NVARCHAR(500) NULL,

    GiaoDichChiId INT NOT NULL UNIQUE,
    GiaoDichThuId INT NOT NULL UNIQUE,

    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'Thành công',
    DaXoa BIT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_CTV_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_CTV_ViNguon FOREIGN KEY (ViNguonId) REFERENCES dbo.Vi(Id),
    CONSTRAINT FK_CTV_ViDich FOREIGN KEY (ViDichId) REFERENCES dbo.Vi(Id),
    CONSTRAINT FK_CTV_GDChi FOREIGN KEY (GiaoDichChiId) REFERENCES dbo.GiaoDich(Id),
    CONSTRAINT FK_CTV_GDThu FOREIGN KEY (GiaoDichThuId) REFERENCES dbo.GiaoDich(Id),

    CONSTRAINT CK_CTV_SoTien CHECK (SoTien > 0),
    CONSTRAINT CK_CTV_TrangThai CHECK (TrangThai IN (N'Thành công', N'Thất bại', N'Đang xử lý'))
);
go
CREATE INDEX IX_CTV_User_Ngay ON dbo.ChuyenTienVi(TaiKhoanId, NgayChuyen DESC);
go
------------------------------------------------------------
-- 11) DONGGOP_MUCTIEU
------------------------------------------------------------
CREATE TABLE dbo.DongGopMucTieu
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MucTieuId INT NOT NULL,
    TaiKhoanId INT NOT NULL,
    GiaoDichId INT NOT NULL UNIQUE,
    SoTien DECIMAL(18,2) NOT NULL,
    NgayDongGop DATE NOT NULL,
    GhiChu NVARCHAR(500) NULL,
    DaXoa BIT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_DG_MucTieu FOREIGN KEY (MucTieuId) REFERENCES dbo.MucTieuTietKiem(Id),
    CONSTRAINT FK_DG_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_DG_GiaoDich FOREIGN KEY (GiaoDichId) REFERENCES dbo.GiaoDich(Id),
    CONSTRAINT CK_DG_SoTien CHECK (SoTien > 0)
);
go
CREATE INDEX IX_DG_MucTieu_Ngay ON dbo.DongGopMucTieu(MucTieuId, NgayDongGop DESC);
go
------------------------------------------------------------
-- 12) SEED DATA (KHÔNG GO NỮA -> biến không mất)
------------------------------------------------------------
INSERT INTO dbo.TaiKhoan (TenDangNhap, MatKhau, HoTen, Quyen, IsActive)
VALUES
(N'admin', N'admin@2024', N'Quản trị hệ thống', N'Admin', 1),
(N'user1', N'user123', N'Nguyễn Văn A', N'User', 1),
(N'user2', N'user456', N'Trần Thị B', N'User', 1);

DECLARE @AdminId INT = (SELECT Id FROM dbo.TaiKhoan WHERE TenDangNhap=N'admin');
DECLARE @User1Id INT  = (SELECT Id FROM dbo.TaiKhoan WHERE TenDangNhap=N'user1');
DECLARE @User2Id INT  = (SELECT Id FROM dbo.TaiKhoan WHERE TenDangNhap=N'user2');

-- Ví user1
INSERT INTO dbo.Vi (TaiKhoanId, TenVi, LoaiVi, SoDuBanDau, GhiChu)
VALUES
(@User1Id, N'Tiền mặt',    N'Tiền mặt',  5000000,  N'Ví tiền mặt hàng ngày'),
(@User1Id, N'Vietcombank', N'Ngân hàng', 12000000, N'Tài khoản lương'),
(@User1Id, N'MoMo',        N'Ví điện tử', 500000,  N'Ví thanh toán online');

-- Ví user2
INSERT INTO dbo.Vi (TaiKhoanId, TenVi, LoaiVi, SoDuBanDau, GhiChu)
VALUES
(@User2Id, N'Tiền mặt',     N'Tiền mặt',  2000000, N'Ví chính'),
(@User2Id, N'Techcombank',  N'Ngân hàng', 8000000, N'Tài khoản tiền lương');

-- Danh mục user1
INSERT INTO dbo.DanhMuc (TaiKhoanId, TenDanhMuc, Loai, Icon, MauSac, GhiChu)
VALUES
(@User1Id, N'Lương',           'THU', N'fa-money-bill',      N'#00AA00', N'Thu nhập chính'),
(@User1Id, N'Thưởng',          'THU', N'fa-gift',            N'#FFD700', N'Thu nhập thưởng'),
(@User1Id, N'Nạp tiền',        'THU', N'fa-wallet',          N'#3498DB', N'Nạp tiền vào ví'),
(@User1Id, N'Nhận chuyển tiền','THU', N'fa-arrow-down',      N'#16A085', N'Nhận tiền từ ví khác'),
(@User1Id, N'Ăn uống',         'CHI', N'fa-utensils',        N'#FF6B6B', N'Chi tiêu ăn uống'),
(@User1Id, N'Đi lại',          'CHI', N'fa-car',             N'#FF8C00', N'Xăng xe, gửi xe'),
(@User1Id, N'Hóa đơn điện',    'CHI', N'fa-bolt',            N'#F1C40F', N'Tiền điện'),
(@User1Id, N'Mua sắm',         'CHI', N'fa-shopping-bag',    N'#FF69B4', N'Quần áo, đồ dùng'),
(@User1Id, N'Chuyển tiền đi',  'CHI', N'fa-arrow-up',        N'#8E44AD', N'Chuyển sang ví khác'),
(@User1Id, N'Rút tiền',        'CHI', N'fa-money-bill-wave', N'#C0392B', N'Rút tiền từ ví');

-- Danh mục user2
INSERT INTO dbo.DanhMuc (TaiKhoanId, TenDanhMuc, Loai, Icon, MauSac, GhiChu)
VALUES
(@User2Id, N'Lương',    'THU', N'fa-money-bill',   N'#00AA00', N'Thu nhập chính'),
(@User2Id, N'Nạp tiền', 'THU', N'fa-wallet',       N'#3498DB', N'Nạp tiền vào ví'),
(@User2Id, N'Ăn uống',  'CHI', N'fa-utensils',     N'#FF6B6B', N'Chi ăn uống'),
(@User2Id, N'Mua sắm',  'CHI', N'fa-shopping-bag', N'#FF69B4', N'Mua đồ'),
(@User2Id, N'Đi lại',   'CHI', N'fa-car',          N'#FF8C00', N'Đi lại');

-- Lấy ID ví + danh mục để seed giao dịch
DECLARE @U1_ViTienMat INT = (SELECT TOP 1 Id FROM dbo.Vi WHERE TaiKhoanId=@User1Id AND TenVi=N'Tiền mặt' AND DaXoa=0);
DECLARE @U1_ViVCB     INT = (SELECT TOP 1 Id FROM dbo.Vi WHERE TaiKhoanId=@User1Id AND TenVi=N'Vietcombank' AND DaXoa=0);
DECLARE @U1_ViMoMo    INT = (SELECT TOP 1 Id FROM dbo.Vi WHERE TaiKhoanId=@User1Id AND TenVi=N'MoMo' AND DaXoa=0);
DECLARE @U2_ViTienMat INT = (SELECT TOP 1 Id FROM dbo.Vi WHERE TaiKhoanId=@User2Id AND TenVi=N'Tiền mặt' AND DaXoa=0);
DECLARE @U2_ViTCB     INT = (SELECT TOP 1 Id FROM dbo.Vi WHERE TaiKhoanId=@User2Id AND TenVi=N'Techcombank' AND DaXoa=0);

DECLARE @U1_DM_Luong  INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User1Id AND TenDanhMuc=N'Lương' AND Loai='THU' AND DaXoa=0);
DECLARE @U1_DM_Thuong INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User1Id AND TenDanhMuc=N'Thưởng' AND Loai='THU' AND DaXoa=0);
DECLARE @U1_DM_Nap    INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User1Id AND TenDanhMuc=N'Nạp tiền' AND Loai='THU' AND DaXoa=0);
DECLARE @U1_DM_AnUong INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User1Id AND TenDanhMuc=N'Ăn uống' AND Loai='CHI' AND DaXoa=0);
DECLARE @U1_DM_Dien   INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User1Id AND TenDanhMuc=N'Hóa đơn điện' AND Loai='CHI' AND DaXoa=0);
DECLARE @U1_DM_DiLai  INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User1Id AND TenDanhMuc=N'Đi lại' AND Loai='CHI' AND DaXoa=0);

DECLARE @U2_DM_Luong  INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User2Id AND TenDanhMuc=N'Lương' AND Loai='THU' AND DaXoa=0);
DECLARE @U2_DM_AnUong INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User2Id AND TenDanhMuc=N'Ăn uống' AND Loai='CHI' AND DaXoa=0);
DECLARE @U2_DM_MuaSam INT = (SELECT TOP 1 Id FROM dbo.DanhMuc WHERE TaiKhoanId=@User2Id AND TenDanhMuc=N'Mua sắm' AND Loai='CHI' AND DaXoa=0);

-- Giao dịch user1
INSERT INTO dbo.GiaoDich (TaiKhoanId, ViId, DanhMucId, SoTien, LoaiGD, NgayGD, GhiChu)
VALUES
(@User1Id, @U1_ViVCB,     @U1_DM_Luong,  15000000, 'THU', '2024-12-01', N'Lương tháng 12'),
(@User1Id, @U1_ViTienMat, @U1_DM_AnUong,   150000, 'CHI', '2024-12-02', N'Ăn trưa'),
(@User1Id, @U1_ViTienMat, @U1_DM_Dien,     400000, 'CHI', '2024-12-05', N'Tiền điện tháng 11'),
(@User1Id, @U1_ViTienMat, @U1_DM_DiLai,    200000, 'CHI', '2024-12-06', N'Xăng xe'),
(@User1Id, @U1_ViMoMo,    @U1_DM_Nap,      500000, 'THU', '2024-12-08', N'Nạp MoMo'),
(@User1Id, @U1_ViVCB,     @U1_DM_Thuong,  2000000, 'THU', '2024-12-08', N'Thưởng kinh doanh');

-- Giao dịch user2
INSERT INTO dbo.GiaoDich (TaiKhoanId, ViId, DanhMucId, SoTien, LoaiGD, NgayGD, GhiChu)
VALUES
(@User2Id, @U2_ViTCB,     @U2_DM_Luong, 12000000, 'THU', '2024-12-01', N'Lương tháng 12'),
(@User2Id, @U2_ViTienMat, @U2_DM_AnUong,  100000, 'CHI', '2024-12-03', N'Ăn trưa'),
(@User2Id, @U2_ViTienMat, @U2_DM_MuaSam,  500000, 'CHI', '2024-12-05', N'Mua quần áo');

-- Ngân sách user1
INSERT INTO dbo.NganSach (TaiKhoanId, DanhMucId, SoTienGioiHan, TGBatDau, TGKetThuc, TrangThai, GhiChu)
VALUES
(@User1Id, @U1_DM_AnUong, 3000000, '2024-12-01', '2024-12-31', N'Hoạt động', N'Ngân sách ăn uống tháng 12'),
(@User1Id, @U1_DM_Dien,    500000, '2024-12-01', '2024-12-31', N'Hoạt động', N'Ngân sách tiền điện');

-- Mục tiêu tiết kiệm
INSERT INTO dbo.MucTieuTietKiem (TaiKhoanId, TenMucTieu, SoTienCanDat, SoTienDaDat, MoTa, HanHoanThanh, TrangThai)
VALUES
(@User1Id, N'Du lịch Đà Nẵng', 10000000, 3000000, N'Chuyến du lịch 5 ngày', '2025-06-01', N'Đang thực hiện'),
(@User1Id, N'Mua laptop mới',  25000000, 8000000, N'Laptop Dell XPS',      '2025-01-31', N'Đang thực hiện'),
(@User2Id, N'Mua xe máy',      15000000, 5000000, N'Xe máy',               '2025-03-01', N'Đang thực hiện');

-- Thông báo mẫu
DECLARE @NS_AnUong_U1 INT = (SELECT TOP 1 Id FROM dbo.NganSach WHERE TaiKhoanId=@User1Id AND DanhMucId=@U1_DM_AnUong);
DECLARE @MT_DN_U1 INT = (SELECT TOP 1 Id FROM dbo.MucTieuTietKiem WHERE TaiKhoanId=@User1Id AND TenMucTieu=N'Du lịch Đà Nẵng');

INSERT INTO dbo.ThongBao (TaiKhoanId, TieuDe, NoiDung, Loai, NganSachId, MucTieuId, TrangThai)
VALUES
(@User1Id, N'Cảnh báo ngân sách', N'Bạn đã phát sinh chi tiêu cho ngân sách ăn uống.', N'Cảnh báo', @NS_AnUong_U1, NULL, N'Chưa xem'),
(@User1Id, N'Tiến độ mục tiêu',   N'Bạn đang tiến gần mục tiêu Du lịch Đà Nẵng.',     N'Thông tin', NULL, @MT_DN_U1, N'Chưa xem');

------------------------------------------------------------
-- 13) CHECK NHANH
------------------------------------------------------------
SELECT * FROM dbo.TaiKhoan;
SELECT * FROM dbo.Vi ORDER BY TaiKhoanId, Id;
SELECT * FROM dbo.DanhMuc ORDER BY TaiKhoanId, Loai, TenDanhMuc;
SELECT TOP 50 * FROM dbo.GiaoDich ORDER BY NgayGD DESC, Id DESC;
SELECT * FROM dbo.NganSach ORDER BY TaiKhoanId, Id;
SELECT * FROM dbo.MucTieuTietKiem ORDER BY TaiKhoanId, Id;
SELECT TOP 50 * FROM dbo.ThongBao ORDER BY ThoiGianGui DESC, Id DESC;