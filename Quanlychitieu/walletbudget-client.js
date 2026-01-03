(function () {
  "use strict";

  /* ===============================
   * CẤU HÌNH BASE URL (GATEWAY)
   * =============================== */
  var GATEWAY_BASE = "https://localhost:7166";

  // Login (Auth)
  var LOGIN_API_BASE = GATEWAY_BASE + "/login/api";

  // Wallet + Budget + Category + Transaction
  var WALLET_API_BASE = GATEWAY_BASE + "/walletbudget/api";

  /* ===============================
   * MODULE
   * =============================== */
  angular
    .module("viApp")
    .factory("WalletBudgetApi", function ($http) {

      /* ===============================
       * VÍ (WALLETS)
       * =============================== */

      // GET: /api/wallets
      function layDanhSachVi(taiKhoanId, includeDeleted) {
        return $http.get(
          WALLET_API_BASE + "/wallets",
          {
            params: {
              taiKhoanId: taiKhoanId,
              includeDeleted: !!includeDeleted
            }
          }
        ).then(res => res.data);
      }

      // POST: /api/wallets
      function taoVi(payload) {
        return $http.post(
          WALLET_API_BASE + "/wallets",
          payload
        ).then(res => res.data);
      }

      // PUT: /api/wallets/{id}
      function suaVi(id, taiKhoanId, payload) {
        return $http.put(
          WALLET_API_BASE + "/wallets/" + id,
          payload,
          {
            params: { taiKhoanId: taiKhoanId }
          }
        ).then(res => res.data);
      }

      // PATCH: /api/wallets/{id}/lock
      function khoaMoVi(id, payload) {
        // payload: { taiKhoanId, isLocked: true|false }
        return $http.patch(
          WALLET_API_BASE + "/wallets/" + id + "/lock",
          payload
        ).then(res => res.data);
      }

      // DELETE: /api/wallets/{id}
      function xoaVi(id, taiKhoanId) {
        return $http.delete(
          WALLET_API_BASE + "/wallets/" + id,
          {
            params: { taiKhoanId: taiKhoanId }
          }
        ).then(res => res.data);
      }

      /* ===============================
       * DANH MỤC (THU)
       * =============================== */

      // GET: /api/categories?taiKhoanId=&loai=THU
      function layDanhMucThu(taiKhoanId) {
        return $http.get(
          WALLET_API_BASE + "/categories",
          {
            params: {
              taiKhoanId: taiKhoanId,
              loai: "THU",
              includeDeleted: false
            }
          }
        ).then(res => res.data);
      }

      /* ===============================
       * GIAO DỊCH (NẠP TIỀN = THU)
       * =============================== */

      // POST: /api/transactions
      function taoGiaoDich(payload) {
        /*
          payload mẫu:
          {
            taiKhoanId,
            viId,
            danhMucId,
            soTien,
            loaiGD: "THU",
            ngayGD: "YYYY-MM-DD",
            ghiChu
          }
        */
        return $http.post(
          WALLET_API_BASE + "/transactions",
          payload
        ).then(res => res.data);
      }

      /* ===============================
       * CHUYỂN TIỀN
       * =============================== */

      // POST: /api/wallettransfer/transfer
      function chuyenTien(payload) {
        /*
          payload mẫu:
          {
            taiKhoanId,
            viNguonId,
            viDichId,
            soTien,
            ngay: "YYYY-MM-DD",
            ghiChu
          }
        */
        return $http.post(
          WALLET_API_BASE + "/wallettransfer/transfer",
          payload
        ).then(res => res.data);
      }

      /* ===============================
       * EXPORT API
       * =============================== */
      return {
        layDanhSachVi: layDanhSachVi,
        taoVi: taoVi,
        suaVi: suaVi,
        khoaMoVi: khoaMoVi,
        xoaVi: xoaVi,
        layDanhMucThu: layDanhMucThu,
        taoGiaoDich: taoGiaoDich,
        chuyenTien: chuyenTien
      };
    });

})();
