(function () {
  "use strict";

  var GATEWAY_BASE = "https://localhost:7166";
  var apiBase = GATEWAY_BASE + "/thongkebaocao/api/ngansach";

  angular.module("walletApp", [])

    /* =========================
       INTERCEPTOR
    ========================== */
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

    /* =========================
       API
    ========================== */
    .factory("NganSachApi", function ($http) {
      return {
        getAll: (taiKhoanId) =>
          $http.get(apiBase, { params: { taiKhoanId } }),
        create: (p) => $http.post(apiBase, p),
        update: (id, taiKhoanId, p) =>
          $http.put(apiBase + "/" + id, p, { params: { taiKhoanId } }),
        remove: (id, taiKhoanId) =>
          $http.delete(apiBase + "/" + id, { params: { taiKhoanId } })
      };
    })

    .factory("DanhMucApi", function ($http) {
      return {
        getAll: (taiKhoanId) =>
          $http.get(GATEWAY_BASE + "/walletbudget/api/categories",
            { params: { taiKhoanId } })
      };
    })

    /* =========================
       CONTROLLER
    ========================== */
    .controller("NganSachCtrl", function (NganSachApi, DanhMucApi) {
      var vm = this;

      /* ===== AUTH ===== */
      if (!AUTH.requireAuth()) return;

      var user = AUTH.getUser();
      if (!user || !user.id) {
        AUTH.redirectAfterLogin();
        return;
      }

      vm.taiKhoanId = user.id;
      vm.tenNguoiDung = user.hoTen || user.tenDangNhap;

      /* ===== STATE ===== */
      vm.danhSach = [];
      vm.danhMucList = [];
      vm.filter = { keyword: "" };
      vm.loading = false;

      vm.isModalOpen = false;
      vm.modalMode = "create";
      vm.modalTitle = "";

      /* ===== FORM ===== */
      vm.resetForm = function () {
        vm.form = {
          id: "",
          danhMucId: "",
          soTienGioiHan: "",
          tgBatDau: "",
          tgKetThuc: "",
          ghiChu: ""
        };
      };

      /* ===== DATE HELPER (QUAN TRỌNG) ===== */
      function toDateInputValue(date) {
        if (!date) return "";
        var d = new Date(date);
        if (isNaN(d.getTime())) return "";
        return d.toISOString().slice(0, 10); // yyyy-MM-dd
      }

      /* ===== HELPERS ===== */
      vm.layTenDanhMuc = function (id) {
        var dm = vm.danhMucList.find(x => x.id == id);
        return dm ? dm.tenDanhMuc : "(Không rõ)";
      };

      /* ===== LOAD DATA ===== */
      vm.taiDanhMuc = function () {
        DanhMucApi.getAll(vm.taiKhoanId)
          .then(res => vm.danhMucList = res.data || []);
      };

      vm.taiDanhSach = function () {
        vm.loading = true;
        NganSachApi.getAll(vm.taiKhoanId)
          .then(res => vm.danhSach = res.data || [])
          .finally(() => vm.loading = false);
      };

      /* ===== MODAL ===== */
      vm.moModalThem = function () {
        vm.modalMode = "create";
        vm.modalTitle = "Thêm ngân sách";
        vm.resetForm();
        vm.isModalOpen = true;
      };

      vm.moModalXem = function (ns) {
        vm.modalMode = "view";
        vm.modalTitle = "Xem ngân sách";

        vm.form = angular.copy(ns);
        vm.form.tgBatDau = toDateInputValue(ns.tgBatDau);
        vm.form.tgKetThuc = toDateInputValue(ns.tgKetThuc);

        vm.isModalOpen = true;
      };

      vm.moModalSua = function (ns) {
        vm.modalMode = "edit";
        vm.modalTitle = "Sửa ngân sách";

        vm.form = angular.copy(ns);
        vm.form.tgBatDau = toDateInputValue(ns.tgBatDau);
        vm.form.tgKetThuc = toDateInputValue(ns.tgKetThuc);

        vm.isModalOpen = true;
      };

      /* ===== SUBMIT ===== */
      vm.submitModal = function () {
        var payload = {
          ...vm.form,
          tgBatDau: vm.form.tgBatDau
            ? new Date(vm.form.tgBatDau).toISOString()
            : null,
          tgKetThuc: vm.form.tgKetThuc
            ? new Date(vm.form.tgKetThuc).toISOString()
            : null,
          taiKhoanId: vm.taiKhoanId
        };

        if (vm.modalMode === "create") {
          NganSachApi.create(payload)
            .then(() => {
              vm.dongModal();
              vm.taiDanhSach();
            });
        }

        if (vm.modalMode === "edit") {
          NganSachApi.update(vm.form.id, vm.taiKhoanId, payload)
            .then(() => {
              vm.dongModal();
              vm.taiDanhSach();
            });
        }
      };

      vm.xoaNganSach = function (ns) {
        if (!confirm("Xóa ngân sách này?")) return;
        NganSachApi.remove(ns.id, vm.taiKhoanId)
          .then(() => vm.taiDanhSach());
      };

      vm.dongModal = function () {
        vm.isModalOpen = false;
      };

      /* ===== INIT ===== */
      vm.taiDanhMuc();
      vm.taiDanhSach();
    });

})();
