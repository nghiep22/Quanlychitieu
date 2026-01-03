(function () {
  "use strict";

  var GATEWAY_BASE = "https://localhost:7166";
  var apiBase = GATEWAY_BASE + "/thongkebaocao/api/muctieutietkiem";

  angular.module("walletApp", [])
    .config(function ($httpProvider) {
      $httpProvider.interceptors.push(function () {
        return {
          request: function (config) {
            var token = localStorage.getItem("token");
            if (token) config.headers.Authorization = "Bearer " + token;
            return config;
          }
        };
      });
    })
    .factory("MucTieuApi", function ($http) {
      return {
        getAll: (taiKhoanId) => $http.get(apiBase, { params: { taiKhoanId } }),
        create: (dto) => $http.post(apiBase, dto),
        remove: (id, taiKhoanId) => $http.delete(apiBase + "/" + id, { params: { taiKhoanId } }),
        addMoney: (id, taiKhoanId, soTien) => 
            $http.post(apiBase + "/" + id + "/add-money", { soTienNap: soTien }, { params: { taiKhoanId } })
      };
    })
    .controller("MucTieuCtrl", function (MucTieuApi, $scope) {
      var vm = this;
      
      // Lấy User ID từ AUTH client hoặc mặc định
      var user = (typeof AUTH !== 'undefined') ? AUTH.getUser() : { id: 1, hoTen: "Người dùng" };
      vm.taiKhoanId = user.id;
      vm.tenNguoiDung = user.hoTen;

      vm.danhSach = [];
      vm.form = {};
      vm.isModalOpen = false;

      vm.taiDanhSach = function () {
        vm.loading = true;
        MucTieuApi.getAll(vm.taiKhoanId)
          .then(res => { vm.danhSach = res.data; })
          .catch(err => console.error("Lỗi tải danh sách:", err))
          .finally(() => { vm.loading = false; });
      };

      vm.moModalThem = function () {
        vm.form = { tenMucTieu: "", soTienCanDat: null, hanHoanThanh: null, moTa: "" };
        vm.isModalOpen = true;
      };

      vm.dongModal = function () { vm.isModalOpen = false; };

      vm.submitModal = function () {
        var soTien = Number(vm.form.soTienCanDat);

        if (isNaN(soTien) || soTien <= 0) {
          alert("Lỗi: Số tiền cần đạt phải lớn hơn 0.");
          return;
        }

        // Payload khớp chính xác với MucTieuTietKiemCreateDto.cs
        var payload = {
          taiKhoanId: vm.taiKhoanId,
          tenMucTieu: vm.form.tenMucTieu,
          soTienCanDat: soTien,
          moTa: vm.form.moTa || "",
          hanHoanThanh: vm.form.hanHoanThanh ? new Date(vm.form.hanHoanThanh).toISOString() : null
        };

        MucTieuApi.create(payload)
          .then(function (res) {
            alert("Tạo mục tiêu thành công!");
            vm.dongModal();
            vm.taiDanhSach();
          })
          .catch(function (err) {
            alert("Lỗi hệ thống: " + (err.data?.message || "Dữ liệu không hợp lệ"));
          });
      };

      vm.xoaMucTieu = function (goal) {
        if (confirm("Xóa mục tiêu: " + goal.tenMucTieu + "?")) {
          MucTieuApi.remove(goal.id, vm.taiKhoanId).then(() => vm.taiDanhSach());
        }
      };

      vm.taiDanhSach();
    });
})();