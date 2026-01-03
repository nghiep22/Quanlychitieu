(function () {
  "use strict";

  angular
    .module("viApp", [])
    .config(function ($httpProvider) {
      // Tự động gắn Bearer token cho mọi request
      $httpProvider.interceptors.push(function () {
        return {
          request: function (config) {
            var token = (window.AUTH && AUTH.getToken) ? AUTH.getToken() : null;
            if (token) config.headers.Authorization = "Bearer " + token;
            return config;
          }
        };
      });
    })
    .controller("ViCtrl", function ($scope, $timeout, WalletBudgetApi) {

      // ====== STATE ======
      $scope.currentUser = null;
      $scope.taiKhoanId = 0;

      $scope.dangTai = false;
      $scope.thongBaoOk = "";
      $scope.thongBaoLoi = "";

      $scope.tuKhoaTim = "";

      $scope.danhSachVi = [];
      $scope.danhSachDanhMucThu = [];

      // modal
      $scope.modalTaoVi = false;
      $scope.modalSuaVi = false;
      $scope.modalNapTien = false;

      $scope.formTaoVi = { tenVi: "", loaiVi: "", soDuBanDau: 0, ghiChu: "" };
      $scope.viDangSua = null;
      $scope.formSuaVi = { tenVi: "", loaiVi: "", soDuBanDau: 0, ghiChu: "" };

      $scope.viDangNap = null;
      $scope.formNapTien = { soTien: null, ngayGD: "", danhMucId: null, ghiChu: "" };

      $scope.formChuyenTien = { viNguonId: null, viDichId: null, soTien: null, ngay: "", ghiChu: "" };

      // ====== HELPER ======
      function setOk(msg) {
        $scope.thongBaoOk = msg || "";
        $scope.thongBaoLoi = "";
        if (msg) $timeout(function () { $scope.thongBaoOk = ""; }, 3500);
      }

      function setLoi(msg) {
        $scope.thongBaoLoi = msg || "";
        $scope.thongBaoOk = "";
      }

      function homNayISO() {
        var d = new Date();
        var mm = String(d.getMonth() + 1).padStart(2, "0");
        var dd = String(d.getDate()).padStart(2, "0");
        return d.getFullYear() + "-" + mm + "-" + dd;
      }

      $scope.dinhDangTien = function (so) {
        so = Number(so || 0);
        return so.toLocaleString("vi-VN");
      };


      // ====== TINH SO DU HIEN TAI (KHONG CAN THEM COT DB) ======
      function unwrapList(data) {
        if (Array.isArray(data)) return data;
        if (!data) return [];
        // hỗ trợ nhiều kiểu trả về: items/Items/data/Data/result/results/value/records...
        return data.items || data.Items || data.data || data.Data ||
               data.result || data.results || data.value || data.records ||
               (data.payload && (data.payload.items || data.payload.data)) ||
               [];
      }

      function apDungSoDuHienTai(danhSachVi, danhSachGiaoDich) {
        danhSachVi = danhSachVi || [];
        danhSachGiaoDich = danhSachGiaoDich || [];

        var mapDelta = {}; // viId -> tong (THU - CHI)

        for (var i = 0; i < danhSachGiaoDich.length; i++) {
          var gd = danhSachGiaoDich[i] || {};
          var viId = Number(gd.viId || gd.ViId || gd.walletId || gd.WalletId || 0);
          if (!viId) continue;

          var loai = String(gd.loaiGD || gd.LoaiGD || gd.loai || gd.Loai || "").toUpperCase();
          var soTien = Number(gd.soTien || gd.SoTien || 0);
          if (!soTien) continue;

          var delta = 0;
          if (loai === "THU") delta = soTien;
          else if (loai === "CHI") delta = -soTien;

          mapDelta[viId] = (mapDelta[viId] || 0) + delta;
        }

        for (var j = 0; j < danhSachVi.length; j++) {
          var v = danhSachVi[j] || {};
          var id = Number(v.id || v.Id || 0);
          var base = Number(v.soDuBanDau || v.SoDuBanDau || 0);
          v.soDuHienTai = base + (mapDelta[id] || 0);
        }
      }

      $scope.locTimKiemVi = function (vi) {
        if (!$scope.tuKhoaTim) return true;
        var t = ($scope.tuKhoaTim || "").toLowerCase();
        var ten = (vi.tenVi || "").toLowerCase();
        var loai = (vi.loaiVi || "").toLowerCase();
        return ten.includes(t) || loai.includes(t) || String(vi.id || "").includes(t);
      };

      function isKhoa(vi) {
        var tt = (vi && vi.trangThai) ? String(vi.trangThai) : "";
        return tt.toLowerCase() === "khóa" || tt.toLowerCase() === "khoa";
      }

      // ====== AUTH ======
      function kiemTraDangNhap() {
        if (!window.AUTH || !AUTH.getUser || !AUTH.getToken) {
          setLoi("Thieu auth-client.js. Kiem tra file auth-client.js co trong thu muc.");
          return false;
        }

        var user = AUTH.getUser();
        var token = AUTH.getToken();

        if (!user || !token) {
          window.location.href = "dangnhap.html";
          return false;
        }

        $scope.currentUser = user;
        $scope.taiKhoanId = Number(user.id || user.Id || 0);

        if (!$scope.taiKhoanId) {
          setLoi("Khong lay duoc taiKhoanId tu currentUser. Kiem tra API login tra ve user.id");
          return false;
        }

        return true;
      }

      $scope.dangXuat = function (e) {
        if (e) e.preventDefault();
        if (window.AUTH && AUTH.clearSession) AUTH.clearSession();
        window.location.href = "index.html";
      };

      // ====== LOAD DATA ======
      function taiDuLieu() {
        $scope.dangTai = true;
        setLoi("");

        var p1 = WalletBudgetApi.layDanhSachVi($scope.taiKhoanId, false)
          .then(function (data) {
            $scope.danhSachVi = Array.isArray(data) ? data : (data.items || data.data || []);
          });

        var p2 = WalletBudgetApi.layDanhMucThu($scope.taiKhoanId)
          .then(function (data) {
            $scope.danhSachDanhMucThu = Array.isArray(data) ? data : (data.items || data.data || []);
          })
          .catch(function () {
            $scope.danhSachDanhMucThu = [];
          });


        // ✅ Lấy giao dịch để tính số dư hiện tại cho từng ví (THU - CHI)
        // Không cần thêm cột SoDuHienTai trong DB.
        var p3 = (WalletBudgetApi.layDanhSachGiaoDich ? WalletBudgetApi.layDanhSachGiaoDich({
          taiKhoanId: $scope.taiKhoanId,
          page: 1,
          pageSize: 10000,
          sort: "NgayGD_desc",
          includeDeleted: false
        }) : Promise.reject({ message: "WalletBudgetApi.chuaCoLayDanhSachGiaoDich" }))
          .then(function (data) {
            $scope.__giaoDichTaiVi = unwrapList(data);
          })
          .catch(function () {
            $scope.__giaoDichTaiVi = [];
          });

        Promise.all([p1, p2, p3].map(chuyenPromiseAngular))
          .then(function () {
            $scope.dangTai = false;

            // ✅ áp dụng số dư hiện tại cho ví (SoDuBanDau + THU - CHI)
            apDungSoDuHienTai($scope.danhSachVi, $scope.__giaoDichTaiVi);

            // set default danhMucId neu co
            if ($scope.danhSachDanhMucThu.length > 0 && !$scope.formNapTien.danhMucId) {
              $scope.formNapTien.danhMucId = $scope.danhSachDanhMucThu[0].id;
            }

            $scope.$applyAsync();
          })
          .catch(function (err) {
            $scope.dangTai = false;
            var msg = (err && err.data && err.data.message) ? err.data.message : (err.message || "Loi tai du lieu.");
            setLoi(msg);
            $scope.$applyAsync();
          });
      }

      // Chuyen $http promise -> native Promise de Promise.all hoat dong on dinh
      function chuyenPromiseAngular(p) {
        return new Promise(function (resolve, reject) {
          p.then(resolve).catch(reject);
        });
      }

      $scope.taiLai = function () {
        taiDuLieu();
      };

      // ====== TAO VI ======
      $scope.moModalTaoVi = function () {
        $scope.formTaoVi = { tenVi: "", loaiVi: "", soDuBanDau: 0, ghiChu: "" };
        $scope.modalTaoVi = true;
      };

      $scope.dongModalTaoVi = function () {
        $scope.modalTaoVi = false;
      };

      $scope.taoVi = function () {
        setLoi("");
        var tenVi = ($scope.formTaoVi.tenVi || "").trim();
        var loaiVi = ($scope.formTaoVi.loaiVi || "").trim();
        var soDuBanDau = Number($scope.formTaoVi.soDuBanDau || 0);

        if (!tenVi) return setLoi("Vui long nhap ten vi.");
        if (!loaiVi) return setLoi("Vui long chon loai vi.");
        if (soDuBanDau < 0) return setLoi("So du ban dau phai >= 0.");

        var payload = {
          taiKhoanId: $scope.taiKhoanId,
          tenVi: tenVi,
          loaiVi: loaiVi,
          soDuBanDau: soDuBanDau,
          ghiChu: ($scope.formTaoVi.ghiChu || "").trim()
        };

        $scope.dangTai = true;
        WalletBudgetApi.taoVi(payload)
          .then(function () {
            $scope.dangTai = false;
            $scope.modalTaoVi = false;
            setOk("Tao vi thanh cong.");
            taiDuLieu();
            $scope.$applyAsync();
          })
          .catch(function (err) {
            $scope.dangTai = false;
            var msg = (err && err.data && err.data.message) ? err.data.message : (err.message || "Tao vi that bai.");
            setLoi(msg);
            $scope.$applyAsync();
          });
      };

      // ====== SUA VI ======
      $scope.moModalSuaVi = function (vi) {
        if (!vi) return;
        $scope.viDangSua = vi;
        $scope.formSuaVi = {
          tenVi: vi.tenVi || "",
          loaiVi: vi.loaiVi || "",
          soDuBanDau: Number(vi.soDuBanDau || 0),
          ghiChu: vi.ghiChu || ""
        };
        $scope.modalSuaVi = true;
      };

      $scope.dongModalSuaVi = function () {
        $scope.modalSuaVi = false;
        $scope.viDangSua = null;
      };

      $scope.suaVi = function () {
        setLoi("");
        if (!$scope.viDangSua) return setLoi("Khong co vi duoc chon.");

        var id = Number($scope.viDangSua.id || 0);
        var tenVi = ($scope.formSuaVi.tenVi || "").trim();
        var loaiVi = ($scope.formSuaVi.loaiVi || "").trim();
        var soDuBanDau = Number($scope.formSuaVi.soDuBanDau || 0);

        if (!id) return setLoi("ID vi khong hop le.");
        if (!tenVi) return setLoi("Vui long nhap ten vi.");
        if (!loaiVi) return setLoi("Vui long chon loai vi.");
        if (soDuBanDau < 0) return setLoi("So du ban dau phai >= 0.");

        var payload = {
          tenVi: tenVi,
          loaiVi: loaiVi,
          soDuBanDau: soDuBanDau,
          ghiChu: ($scope.formSuaVi.ghiChu || "").trim()
        };

        $scope.dangTai = true;
        WalletBudgetApi.suaVi(id, $scope.taiKhoanId, payload)
          .then(function () {
            $scope.dangTai = false;
            $scope.modalSuaVi = false;
            setOk("Cap nhat vi thanh cong.");
            taiDuLieu();
            $scope.$applyAsync();
          })
          .catch(function (err) {
            $scope.dangTai = false;
            var msg = (err && err.data && err.data.message) ? err.data.message : (err.message || "Cap nhat vi that bai.");
            setLoi(msg);
            $scope.$applyAsync();
          });
      };

      // ====== KHOA/MO VI ======
      $scope.khoaMoVi = function (vi) {
        setLoi("");
        if (!vi) return;
        var id = Number(vi.id || 0);
        if (!id) return setLoi("ID vi khong hop le.");

        var muonKhoa = !isKhoa(vi); // đang hoạt động -> khóa, đang khóa -> mở
        var payload = { taiKhoanId: $scope.taiKhoanId, isLocked: muonKhoa };

        $scope.dangTai = true;
        WalletBudgetApi.khoaMoVi(id, payload)
          .then(function () {
            $scope.dangTai = false;
            setOk(muonKhoa ? "Da khoa vi." : "Da mo khoa vi.");
            taiDuLieu();
            $scope.$applyAsync();
          })
          .catch(function (err) {
            $scope.dangTai = false;
            var msg = (err && err.data && err.data.message) ? err.data.message : (err.message || "Thao tac khoa/mo that bai.");
            setLoi(msg);
            $scope.$applyAsync();
          });
      };

      // ====== XOA VI (XOA MEM) ======
      $scope.xoaVi = function (vi) {
        setLoi("");
        if (!vi) return;
        var id = Number(vi.id || 0);
        if (!id) return setLoi("ID vi khong hop le.");

        if (!confirm("Ban chac chan muon xoa mem vi '" + (vi.tenVi || ("#" + id)) + "' ?")) return;

        $scope.dangTai = true;
        WalletBudgetApi.xoaVi(id, $scope.taiKhoanId)
          .then(function () {
            $scope.dangTai = false;
            setOk("Da xoa vi (xoa mem).");
            taiDuLieu();
            $scope.$applyAsync();
          })
          .catch(function (err) {
            $scope.dangTai = false;
            var msg = (err && err.data && err.data.message) ? err.data.message : (err.message || "Xoa vi that bai.");
            setLoi(msg);
            $scope.$applyAsync();
          });
      };

      // ====== NAP TIEN (TAO GIAO DICH THU) ======
      $scope.moModalNapTien = function (vi) {
        if (!vi) return;
        if (isKhoa(vi)) return setLoi("Vi dang bi khoa, khong the nap tien.");

        $scope.viDangNap = vi;
        $scope.formNapTien = {
          soTien: null,
          ngayGD: homNayISO(),
          danhMucId: ($scope.danhSachDanhMucThu[0] ? $scope.danhSachDanhMucThu[0].id : null),
          ghiChu: "Nap tien vao vi"
        };
        $scope.modalNapTien = true;
      };

      $scope.dongModalNapTien = function () {
        $scope.modalNapTien = false;
        $scope.viDangNap = null;
      };

      $scope.napTien = function () {
        setLoi("");
        if (!$scope.viDangNap) return setLoi("Khong co vi duoc chon.");

        var soTien = Number($scope.formNapTien.soTien || 0);
        if (soTien <= 0) return setLoi("So tien phai > 0.");

        if (!$scope.formNapTien.danhMucId) {
          return setLoi("Ban can chon danh muc THU (hoac tao danh muc THU trong trang Danh muc).");
        }

        var payload = {
          taiKhoanId: $scope.taiKhoanId,
          viId: Number($scope.viDangNap.id),
          danhMucId: Number($scope.formNapTien.danhMucId),
          soTien: soTien,
          loaiGD: "THU",
          ngayGD: $scope.formNapTien.ngayGD || homNayISO(),
          ghiChu: ($scope.formNapTien.ghiChu || "").trim()
        };

        $scope.dangTai = true;
        WalletBudgetApi.taoGiaoDich(payload)
          .then(function () {
            $scope.dangTai = false;
            $scope.modalNapTien = false;
            setOk("Nap tien thanh cong.");
            taiDuLieu();
            $scope.$applyAsync();
          })
          .catch(function (err) {
            $scope.dangTai = false;
            var msg = (err && err.data && err.data.message) ? err.data.message : (err.message || "Nap tien that bai.");
            setLoi(msg);
            $scope.$applyAsync();
          });
      };

      // ====== CHUYEN TIEN ======
      $scope.chonViNguon = function (vi) {
        if (!vi) return;
        if (isKhoa(vi)) return setLoi("Vi nguon dang bi khoa.");
        $scope.formChuyenTien.viNguonId = vi.id;
      };

      $scope.chonViDich = function (vi) {
        if (!vi) return;
        if (isKhoa(vi)) return setLoi("Vi dich dang bi khoa.");
        $scope.formChuyenTien.viDichId = vi.id;
      };

      $scope.resetChuyenTien = function () {
        $scope.formChuyenTien = { viNguonId: null, viDichId: null, soTien: null, ngay: homNayISO(), ghiChu: "" };
      };

      $scope.chuyenTien = function () {
        setLoi("");

        var viNguonId = Number($scope.formChuyenTien.viNguonId || 0);
        var viDichId = Number($scope.formChuyenTien.viDichId || 0);
        var soTien = Number($scope.formChuyenTien.soTien || 0);

        if (!viNguonId || !viDichId) return setLoi("Vui long chon vi nguon va vi dich.");
        if (viNguonId === viDichId) return setLoi("Vi nguon va vi dich khong duoc trung nhau.");
        if (soTien <= 0) return setLoi("So tien phai > 0.");

        var payload = {
          taiKhoanId: $scope.taiKhoanId,
          viNguonId: viNguonId,
          viDichId: viDichId,
          soTien: soTien,
          ngay: $scope.formChuyenTien.ngay || homNayISO(),
          ghiChu: ($scope.formChuyenTien.ghiChu || "").trim()
        };

        $scope.dangTai = true;
        WalletBudgetApi.chuyenTien(payload)
          .then(function () {
            $scope.dangTai = false;
            setOk("Chuyen tien thanh cong.");
            taiDuLieu();
            $scope.$applyAsync();
          })
          .catch(function (err) {
            $scope.dangTai = false;
            var msg = (err && err.data && err.data.message) ? err.data.message : (err.message || "Chuyen tien that bai. Kiem tra API WalletTransfer.");
            setLoi(msg);
            $scope.$applyAsync();
          });
      };

      // ====== INIT ======
      function khoiTao() {
        if (!kiemTraDangNhap()) return;
        $scope.resetChuyenTien();
        taiDuLieu();
      }

      khoiTao();
    });

})();