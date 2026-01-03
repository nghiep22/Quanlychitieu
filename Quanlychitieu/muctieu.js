(function () {
  "use strict";

  var GATEWAY_BASE = "https://localhost:7166";
  // API cho Mục tiêu và Đóng góp theo đúng Controller của bạn
  var apiMucTieu = GATEWAY_BASE + "/thongkebaocao/api/muctieutietkiem";
  var apiDongGop = GATEWAY_BASE + "/thongkebaocao/api/donggopmuctieu";

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
        // Các hàm cho Mục tiêu
        getAll: (taiKhoanId) => $http.get(apiMucTieu, { params: { taiKhoanId } }),
        create: (dto) => $http.post(apiMucTieu, dto),
        remove: (id, taiKhoanId) => $http.delete(apiMucTieu + "/" + id, { params: { taiKhoanId } }),
        
        // Các hàm cho Đóng góp (Khớp với DongGopMucTieuController.cs)
        createDongGop: (dto) => $http.post(apiDongGop, dto),
        getLichSuByMucTieu: (mucTieuId) => $http.get(apiDongGop + "/muctieu/" + mucTieuId)
      };
    })
    .controller("MucTieuCtrl", function (MucTieuApi, $scope, $q) {
      var vm = this;
      
      var user = (typeof AUTH !== 'undefined') ? AUTH.getUser() : { id: 1, hoTen: "Người dùng" };
      vm.taiKhoanId = user.id;
      vm.tenNguoiDung = user.hoTen;

      vm.danhSach = [];
      vm.lichSuDongGop = [];
      vm.dongGopHienTai = null;
      vm.soTienDongGop = null;

      // 1. Tải danh sách mục tiêu và lịch sử đi kèm
      vm.taiDanhSach = function () {
        vm.loading = true;
        MucTieuApi.getAll(vm.taiKhoanId)
          .then(res => { 
            vm.danhSach = res.data; 
            vm.taiLichSuTongHop(); // Sau khi có mục tiêu thì tải lịch sử
          })
          .catch(err => console.error("Lỗi:", err))
          .finally(() => { vm.loading = false; });
      };

      // 2. Tải lịch sử đóng góp cho tất cả mục tiêu hiện có
      vm.taiLichSuTongHop = function() {
        var promises = vm.danhSach.map(goal => MucTieuApi.getLichSuByMucTieu(goal.id));
        
        $q.all(promises).then(responses => {
          vm.lichSuDongGop = [];
          responses.forEach(res => {
            vm.lichSuDongGop = vm.lichSuDongGop.concat(res.data);
          });
        });
      };

      // 3. Xử lý hiển thị textbox đóng góp
      vm.moDongGop = (goal) => { 
        vm.dongGopHienTai = goal; 
        vm.soTienDongGop = null; 
      };
      
      vm.huyDongGop = () => { vm.dongGopHienTai = null; };

      // 4. Xác nhận đóng góp
vm.xacNhanDongGop = function() {
    if (!vm.soTienDongGop || vm.soTienDongGop <= 0) {
        alert("Vui lòng nhập số tiền hợp lệ!");
        return;
    }

    var payload = {
        mucTieuId: vm.dongGopHienTai.id,
        taiKhoanId: vm.taiKhoanId,
        soTien: vm.soTienDongGop,
        ngayDongGop: new Date().toISOString(),
        ghiChu: "Đóng góp tự động: " + vm.dongGopHienTai.tenMucTieu
    };

    MucTieuApi.createDongGop(payload)
        .then(function(res) {
            alert("Thành công! Giao dịch đã được ghi nhận tự động.");
            vm.soTienDongGop = 0;
            vm.hienThiFormDongGop = false; // Đóng modal
            return vm.taiDanhSach(); // Cập nhật lại thanh tiến độ
        })
        .catch(function(err) {
            console.error(err);
            alert("Lỗi: " + (err.data?.message || "Không thể thực hiện đóng góp"));
        });
};
      // 5. Các hàm quản lý mục tiêu khác
      vm.moModalThem = function () {
        vm.form = { tenMucTieu: "", soTienCanDat: null, hanHoanThanh: null, moTa: "" };
        vm.isModalOpen = true;
      };

      vm.dongModal = function () { vm.isModalOpen = false; };

      vm.submitModal = function () {
        var payload = {
          taiKhoanId: vm.taiKhoanId,
          tenMucTieu: vm.form.tenMucTieu,
          soTienCanDat: vm.form.soTienCanDat,
          moTa: vm.form.moTa || "",
          hanHoanThanh: vm.form.hanHoanThanh ? new Date(vm.form.hanHoanThanh).toISOString() : null
        };
        MucTieuApi.create(payload).then(() => {
          vm.dongModal();
          vm.taiDanhSach();
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