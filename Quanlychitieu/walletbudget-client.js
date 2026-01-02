(function () {
  "use strict";

  // ✅ GỌI QUA API GATEWAY (Ocelot)
  // Gateway base: http://localhost:7001
  // Route WalletBudget trên Gateway: /walletbudget/{everything}
  // => API base: http://localhost:7001/walletbudget/api
  var WALLET_API_BASE = "http://localhost:7001/walletbudget/api";

  angular.module("viApp").factory("WalletBudgetApi", function ($http, $q) {

    function goiNhieuEndpoint(danhSachUrl, cauHinh) {
      var deferred = $q.defer();
      var i = 0;

      function thu() {
        if (i >= danhSachUrl.length) {
          deferred.reject(new Error("Khong goi duoc endpoint nao (kiem tra route Controller)."));
          return;
        }

        var url = danhSachUrl[i++];
        $http(angular.extend({}, cauHinh, { url: url }))
          .then(function (res) { deferred.resolve(res.data); })
          .catch(function (err) {
            // 404 -> thử endpoint tiếp theo
            if (err && err.status === 404) return thu();
            deferred.reject(err);
          });
      }

      thu();
      return deferred.promise;
    }

    // ========= VI (WalletsController) =========
    function layDanhSachVi(taiKhoanId, includeDeleted) {
      includeDeleted = !!includeDeleted;
      var urls = [
        WALLET_API_BASE + "/wallets?taiKhoanId=" + taiKhoanId + "&includeDeleted=" + includeDeleted,
        // fallback cho project cũ (nếu bạn từng đặt route khác)
        WALLET_API_BASE + "/vi?taiKhoanId=" + taiKhoanId + "&includeDeleted=" + includeDeleted,
        WALLET_API_BASE + "/wallet?taiKhoanId=" + taiKhoanId + "&includeDeleted=" + includeDeleted
      ];
      return goiNhieuEndpoint(urls, { method: "GET" });
    }

    function taoVi(payload) {
      var urls = [
        WALLET_API_BASE + "/wallets",
        WALLET_API_BASE + "/vi",
        WALLET_API_BASE + "/wallet"
      ];
      return goiNhieuEndpoint(urls, { method: "POST", data: payload });
    }

    function suaVi(id, taiKhoanId, payload) {
      var urls = [
        WALLET_API_BASE + "/wallets/" + id + "?taiKhoanId=" + taiKhoanId,
        WALLET_API_BASE + "/vi/" + id + "?taiKhoanId=" + taiKhoanId,
        WALLET_API_BASE + "/wallet/" + id + "?taiKhoanId=" + taiKhoanId
      ];
      return goiNhieuEndpoint(urls, { method: "PUT", data: payload });
    }

    function khoaMoVi(id, payload) {
      // payload: { taiKhoanId, isLocked: true/false }
      var urls = [
        WALLET_API_BASE + "/wallets/" + id + "/lock",
        WALLET_API_BASE + "/vi/" + id + "/lock",
        WALLET_API_BASE + "/wallet/" + id + "/lock"
      ];
      return goiNhieuEndpoint(urls, { method: "PATCH", data: payload });
    }

    function xoaVi(id, taiKhoanId) {
      var urls = [
        WALLET_API_BASE + "/wallets/" + id + "?taiKhoanId=" + taiKhoanId,
        WALLET_API_BASE + "/vi/" + id + "?taiKhoanId=" + taiKhoanId,
        WALLET_API_BASE + "/wallet/" + id + "?taiKhoanId=" + taiKhoanId
      ];
      return goiNhieuEndpoint(urls, { method: "DELETE" });
    }

    // ========= DANH MUC (THU) =========
    function layDanhMucThu(taiKhoanId) {
      // Theo controller bạn từng làm: GET /api/categories?taiKhoanId=2&loai=THU
      var urls = [
        WALLET_API_BASE + "/categories?taiKhoanId=" + taiKhoanId + "&loai=THU&includeDeleted=false",
        WALLET_API_BASE + "/danhmuc?taiKhoanId=" + taiKhoanId + "&loai=THU&includeDeleted=false"
      ];
      return goiNhieuEndpoint(urls, { method: "GET" });
    }

    // ========= GIAO DICH (NAP TIEN = THU) =========
    function taoGiaoDich(payload) {
      // Gợi ý payload chuẩn:
      // { taiKhoanId, viId, danhMucId, soTien, loaiGD:'THU'|'CHI', ngayGD:'YYYY-MM-DD', ghiChu }
      // (nếu BE bạn đang dùng "loai" / "ngayGiaoDich" thì bạn có thể map lại bên Controller)
      var urls = [
        WALLET_API_BASE + "/transactions",
        WALLET_API_BASE + "/giaodich",
        WALLET_API_BASE + "/transaction"
      ];
      return goiNhieuEndpoint(urls, { method: "POST", data: payload });
    }


    // ========= GIAO DICH (LIST) =========
    function layDanhSachGiaoDich(params) {
      // params: { taiKhoanId, from, to, viId, danhMucId, loai, q, page, pageSize, sort, includeDeleted }
      params = params || {};
      var taiKhoanId = params.taiKhoanId;

      // build querystring
      var qs = [];
      function add(k, v) {
        if (v === undefined || v === null || v === "") return;
        qs.push(encodeURIComponent(k) + "=" + encodeURIComponent(v));
      }

      add("taiKhoanId", taiKhoanId);
      add("from", params.from);
      add("to", params.to);
      add("viId", params.viId);
      add("danhMucId", params.danhMucId);
      add("loai", params.loai);
      add("q", params.q);

      add("page", params.page || 1);
      add("pageSize", params.pageSize || 1000);
      add("sort", params.sort || "NgayGD_desc");
      add("includeDeleted", params.includeDeleted ? "true" : "false");

      var query = qs.length ? ("?" + qs.join("&")) : "";

      var urls = [
        WALLET_API_BASE + "/transactions" + query,
        WALLET_API_BASE + "/giaodich" + query,
        WALLET_API_BASE + "/transaction" + query
      ];
      return goiNhieuEndpoint(urls, { method: "GET" });
    }

    // ========= CHUYEN TIEN =========
    function chuyenTien(payload) {
      // payload: { taiKhoanId, viNguonId, viDichId, soTien, ngay:'YYYY-MM-DD', ghiChu }
      var urls = [
        WALLET_API_BASE + "/wallettransfer/transfer",
        WALLET_API_BASE + "/wallettransfer",
        WALLET_API_BASE + "/transfer"
      ];
      return goiNhieuEndpoint(urls, { method: "POST", data: payload });
    }

    return {
      layDanhSachVi: layDanhSachVi,
      taoVi: taoVi,
      suaVi: suaVi,
      khoaMoVi: khoaMoVi,
      xoaVi: xoaVi,
      layDanhMucThu: layDanhMucThu,
      taoGiaoDich: taoGiaoDich,
      layDanhSachGiaoDich: layDanhSachGiaoDich,
      chuyenTien: chuyenTien
    };
  });

})();