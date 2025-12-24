(function () {
  "use strict";

  // ✅ GỌI QUA API GATEWAY (Ocelot)
  // Gateway đang chạy: http://localhost:7001
  // Route WalletBudget trên Gateway: /walletbudget/{everything}
  // => API base: http://localhost:7001/walletbudget/api
  var apiBase = "http://localhost:7001/walletbudget/api";

  angular
    .module("walletApp", [])

    // (Tuỳ chọn) Nếu bạn có JWT token sau login thì tự gắn Bearer cho mọi request
    .config(function ($httpProvider) {
      $httpProvider.interceptors.push(function () {
        return {
          request: function (config) {
            var token = localStorage.getItem("token"); // nếu bạn lưu token
            if (token) {
              config.headers.Authorization = "Bearer " + token;
            }
            return config;
          }
        };
      });
    })

    .factory("CategoriesApi", function ($http) {
      var base = apiBase + "/categories";

      return {
        getAll: function (taiKhoanId, loai, status, includeDeleted) {
          return $http.get(base, {
            params: {
              taiKhoanId: taiKhoanId,
              loai: loai || null,
              status: status || "Hoạt động",
              includeDeleted: !!includeDeleted
            }
          });
        },
        create: function (payload) {
          return $http.post(base, payload);
        }
      };
    })

    .controller("CategoriesCtrl", function (CategoriesApi) {
      var vm = this;

      vm.loading = false;
      vm.taiKhoanId = Number(localStorage.getItem("taiKhoanId") || 2);

      vm.filter = { loai: "", keyword: "" };
      vm.danhSach = [];

      vm.isModalOpen = false;
      vm.form = { tenDanhMuc: "", loai: "", icon: "", ghiChu: "", mauSac: "" };

      function mauSacMacDinh(loai) {
        if (loai === "THU") return "#00AA00";
        if (loai === "CHI") return "#FF6B6B";
        return "#3498DB";
      }

      vm.moModalThem = function () {
        vm.form = { tenDanhMuc: "", loai: "", icon: "", ghiChu: "", mauSac: "" };
        vm.isModalOpen = true;
      };

      vm.dongModalThem = function () {
        vm.isModalOpen = false;
      };

      vm.taiDanhSach = function () {
        if (!vm.taiKhoanId || vm.taiKhoanId <= 0) {
          alert("taiKhoanId phải > 0. Hãy set localStorage taiKhoanId (vd 2).");
          return;
        }

        vm.loading = true;
        CategoriesApi.getAll(vm.taiKhoanId, vm.filter.loai, "Hoạt động", false)
          .then(function (res) { vm.danhSach = res.data || []; })
          .catch(function (err) {
            console.error(err);
            var msg = (err.data && err.data.message) ? err.data.message : ("HTTP " + err.status);
            alert("Lỗi tải danh mục: " + msg);
          })
          .finally(function () { vm.loading = false; });
      };

      vm.taoDanhMuc = function () {
        if (!vm.taiKhoanId || vm.taiKhoanId <= 0) {
          alert("taiKhoanId phải > 0. Hãy set localStorage taiKhoanId (vd 2).");
          return;
        }

        var payload = {
          taiKhoanId: vm.taiKhoanId,
          tenDanhMuc: vm.form.tenDanhMuc,
          loai: vm.form.loai,
          icon: vm.form.icon,
          mauSac: (vm.form.mauSac && vm.form.mauSac.trim())
            ? vm.form.mauSac.trim()
            : mauSacMacDinh(vm.form.loai),
          ghiChu: vm.form.ghiChu,
          trangThai: "Hoạt động"
        };

        CategoriesApi.create(payload)
          .then(function () {
            vm.dongModalThem();
            vm.taiDanhSach();
          })
          .catch(function (err) {
            console.error(err);
            if (err.status === 409) return alert("Tên danh mục đã tồn tại (cùng TaiKhoanId + Loai).");
            var msg = (err.data && err.data.message) ? err.data.message : ("HTTP " + err.status);
            alert("Lỗi tạo danh mục: " + msg);
          });
      };

      vm.taiDanhSach();
    });
})();
