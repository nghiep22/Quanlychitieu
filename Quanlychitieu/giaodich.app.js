(function () {
  "use strict";

  // =========================
  // CONFIG
  // =========================
  // Mặc định gọi qua API Gateway (giống categories.app.js)
  // Bạn có thể override bằng: window.__WALLETBUDGET_API_BASE = "http://localhost:7001/walletbudget/api";
  var API_BASE = (window.__WALLETBUDGET_API_BASE || "http://localhost:7001/walletbudget/api").replace(/\/+$/, "");

  // Fallback endpoint (nếu BE bạn đặt route khác)
  var ENDPOINTS = {
    wallets: [API_BASE + "/wallets", API_BASE + "/vi", API_BASE + "/wallet"],
    categories: [API_BASE + "/categories", API_BASE + "/danhmuc"],
    transactions: [API_BASE + "/transactions", API_BASE + "/giaodich"]
  };

  // Cache keys
  var CACHE_VI = "cache_danhSachVi";
  var CACHE_DM = "cache_danhSachDanhMuc";

  // =========================
  // AUTH HELPERS
  // =========================
  function layTaiKhoanId() {
    try {
      if (window.AUTH && typeof window.AUTH.getUser === "function") {
        var u = window.AUTH.getUser();
        var id = Number((u && (u.id || u.Id || u.taiKhoanId || u.TaiKhoanId)) || 0);
        if (id) return id;
      }
    } catch (_) { }
    return Number(localStorage.getItem("taiKhoanId") || 0);
  }

  function layToken() {
    try {
      if (window.AUTH && typeof window.AUTH.getToken === "function") {
        return window.AUTH.getToken();
      }
    } catch (_) { }
    return localStorage.getItem("token") || "";
  }

  function headersJson() {
    var h = { "Content-Type": "application/json" };
    var token = layToken();
    if (token) h.Authorization = "Bearer " + token;
    return h;
  }

  // =========================
  // DOM HELPERS
  // =========================
  function byId(id) { return document.getElementById(id); }
  function qs(sel, root) { return (root || document).querySelector(sel); }
  function qsa(sel, root) { return Array.prototype.slice.call((root || document).querySelectorAll(sel)); }

  function showModal(modalEl) { if (modalEl) modalEl.style.display = "flex"; }
  function hideModal(modalEl) { if (modalEl) modalEl.style.display = "none"; }

  function homNayISO() {
    var d = new Date();
    var yyyy = d.getFullYear();
    var mm = String(d.getMonth() + 1).padStart(2, "0");
    var dd = String(d.getDate()).padStart(2, "0");
    return yyyy + "-" + mm + "-" + dd;
  }

  function fmtTien(n) {
    try { return Number(n || 0).toLocaleString("vi-VN"); }
    catch (_) { return String(n || 0); }
  }

  function escapeHtml(str) {
    return String(str || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  function pick(obj, keys) {
    if (!obj) return undefined;
    for (var i = 0; i < keys.length; i++) {
      var k = keys[i];
      if (Object.prototype.hasOwnProperty.call(obj, k) && obj[k] !== undefined) return obj[k];
    }
    return undefined;
  }

  function docCache(key) {
    try {
      var raw = localStorage.getItem(key);
      return raw ? JSON.parse(raw) : null;
    } catch (_) { return null; }
  }

  function luuCache(key, data) {
    try { localStorage.setItem(key, JSON.stringify(data || [])); } catch (_) { }
  }

  // =========================
  // HTTP: thử nhiều endpoint
  // =========================
  function fetchJson(url, opts) {
    return fetch(url, opts).then(function (res) {
      return res.text().then(function (text) {
        var data = null;
        try { data = text ? JSON.parse(text) : null; } catch (_) { data = text; }
        if (!res.ok) {
          var err = new Error("HTTP " + res.status);
          err.status = res.status;
          err.data = data;
          throw err;
        }
        return data;
      });
    });
  }

  function goiNhieuEndpoint(urls, opts) {
    var lastErr = null;
    var p = Promise.reject();
    urls.forEach(function (u) {
      p = p.catch(function () {
        return fetchJson(u, opts).catch(function (e) {
          lastErr = e;
          throw e;
        });
      });
    });
    return p.catch(function () { throw lastErr; });
  }

  // =========================
  // STATE
  // =========================
  var state = {
    taiKhoanId: 0,
    danhSachVi: [],
    danhSachDanhMuc: [],
    danhSachGiaoDich: []
  };

  // =========================
  // ELEMENTS (theo giaodich.html)
  // =========================
  var el = {
    // Header search input (không có id)
    qSearch: qs(".header-search input"),

    // Table body (giao dịch gần đây)
    tbody: (function () {
      // tbody của bảng đầu tiên trong card "Giao dịch gần đây"
      var tables = qsa("main.container .left .card table");
      if (tables && tables.length) return qs("tbody", tables[0]);
      return qs("table tbody");
    })(),

    // Summary
    sumThu: qs(".stat-value.stat-income"),
    sumChi: qs(".stat-value.stat-expense"),
    sumLech: qs(".stat-value.stat-balance"),

    // Button
    btnThem: byId("btnAddTransaction"),

    // Modal Add Transaction
    modalAdd: byId("modalAddTransaction"),
    loaiGD: byId("transactionType"),
    danhMucId: byId("categoryId"),
    viId: byId("walletId"),
    soTien: byId("soTien"),
    ngayGD: byId("ngayGD"),
    ghiChu: byId("ghiChu")
  };

  // Modal controls (close/cancel/form)
  var modalCloseBtns = el.modalAdd ? qsa(".modal-close", el.modalAdd) : [];
  var modalCancelBtn = el.modalAdd ? qs(".modal-cancel", el.modalAdd) : null;
  var modalForm = el.modalAdd ? qs("form", el.modalAdd) : null;

  // =========================
  // INIT
  // =========================
  document.addEventListener("DOMContentLoaded", init);
  // nếu file này được load cuối body thì DOMContentLoaded có thể đã chạy, fallback:
  if (document.readyState === "interactive" || document.readyState === "complete") {
    setTimeout(init, 0);
  }

  var _inited = false;
  function init() {
    if (_inited) return;
    _inited = true;

    state.taiKhoanId = layTaiKhoanId() || Number(localStorage.getItem("taiKhoanId") || 0);

    // Nếu chưa có taiKhoanId: vẫn cho mở modal nhưng sẽ báo khi tạo
    // Nạp cache trước để dropdown có data ngay
    var cVi = docCache(CACHE_VI);
    if (Array.isArray(cVi) && cVi.length) state.danhSachVi = cVi;

    var cDM = docCache(CACHE_DM);
    if (Array.isArray(cDM) && cDM.length) state.danhSachDanhMuc = cDM;

    // Gắn sự kiện
    ganSuKien();

    // Load dữ liệu thật
    taiViVaDanhMuc()
      .then(function () {
        return taiDanhSachGiaoDich();
      })
      .catch(function (e) {
        console.error("[GiaoDich] init error", e);
        // không alert ở init để tránh khó chịu, nhưng log để debug
      });
  }

  function ganSuKien() {
    // Mở modal
    if (el.btnThem && el.modalAdd) {
      el.btnThem.addEventListener("click", function () {
        // đảm bảo có dữ liệu select
        doSelectVi(el.viId, state.danhSachVi, "-- Chọn ví --");
        doDanhMucTheoLoai(el.danhMucId, state.danhSachDanhMuc, (el.loaiGD && el.loaiGD.value) || "");

        if (el.ngayGD) el.ngayGD.value = el.ngayGD.value || homNayISO();
        showModal(el.modalAdd);
      });
    }

    // Close modal
    modalCloseBtns.forEach(function (b) {
      b.addEventListener("click", function (e) {
        e.preventDefault();
        hideModal(el.modalAdd);
      });
    });
    if (modalCancelBtn) {
      modalCancelBtn.addEventListener("click", function (e) {
        e.preventDefault();
        hideModal(el.modalAdd);
      });
    }
    // click nền để đóng
    if (el.modalAdd) {
      el.modalAdd.addEventListener("click", function (e) {
        if (e.target === el.modalAdd) hideModal(el.modalAdd);
      });
    }

    // đổi loại -> lọc danh mục
    if (el.loaiGD) {
      el.loaiGD.addEventListener("change", function () {
        doDanhMucTheoLoai(el.danhMucId, state.danhSachDanhMuc, el.loaiGD.value);
      });
    }

    // submit form
    if (modalForm) {
      modalForm.addEventListener("submit", function (e) {
        e.preventDefault();
        taoGiaoDich();
      });
    }

    // search header -> debounce reload
    if (el.qSearch) {
      var t = null;
      el.qSearch.addEventListener("input", function () {
        if (t) clearTimeout(t);
        t = setTimeout(function () {
          taiDanhSachGiaoDich();
        }, 350);
      });
    }
  }

  // =========================
  // LOAD VI + DANH MUC
  // =========================
  function taiViVaDanhMuc() {
    var taiKhoanId = state.taiKhoanId || Number(localStorage.getItem("taiKhoanId") || 0);

    var p1 = taiDanhSachVi(taiKhoanId).then(function (ds) {
      state.danhSachVi = chuanHoaDanhSach(ds);
      luuCache(CACHE_VI, state.danhSachVi);
      // update select nếu modal đang mở
      doSelectVi(el.viId, state.danhSachVi, "-- Chọn ví --");
    });

    var p2 = taiDanhSachDanhMuc(taiKhoanId).then(function (ds) {
      state.danhSachDanhMuc = chuanHoaDanhSach(ds);
      luuCache(CACHE_DM, state.danhSachDanhMuc);
      // update select nếu modal đang mở
      doDanhMucTheoLoai(el.danhMucId, state.danhSachDanhMuc, (el.loaiGD && el.loaiGD.value) || "");
    });

    return Promise.all([p1, p2]).then(function () { return true; });
  }

  function taiDanhSachVi(taiKhoanId) {
    if (!taiKhoanId) return Promise.resolve([]);
    var urls = ENDPOINTS.wallets.map(function (u) {
      // wallets GET thường: /wallets?taiKhoanId=2&includeDeleted=false
      // một số BE: /vi?taiKhoanId=2...
      var sep = u.indexOf("?") >= 0 ? "&" : "?";
      return u + sep + "taiKhoanId=" + encodeURIComponent(taiKhoanId) + "&includeDeleted=false";
    });
    return goiNhieuEndpoint(urls, { method: "GET", headers: headersJson() })
      .then(function (data) {
        return Array.isArray(data) ? data : (data && (data.items || data.data) ? (data.items || data.data) : []);
      });
  }

  function taiDanhSachDanhMuc(taiKhoanId) {
    if (!taiKhoanId) return Promise.resolve([]);
    var urls = ENDPOINTS.categories.map(function (u) {
      // categories GET thường: /categories?taiKhoanId=2&loai=&status=Hoạt động&includeDeleted=false
      var sep = u.indexOf("?") >= 0 ? "&" : "?";
      return (
        u + sep +
        "taiKhoanId=" + encodeURIComponent(taiKhoanId) +
        "&includeDeleted=false"
      );
    });
    return goiNhieuEndpoint(urls, { method: "GET", headers: headersJson() })
      .then(function (data) {
        return Array.isArray(data) ? data : (data && (data.items || data.data) ? (data.items || data.data) : []);
      });
  }

  // =========================
  // LOAD GIAO DICH
  // =========================
  function taiDanhSachGiaoDich() {
    var taiKhoanId = state.taiKhoanId || Number(localStorage.getItem("taiKhoanId") || 0);
    if (!taiKhoanId) {
      // chưa đăng nhập: clear bảng
      state.danhSachGiaoDich = [];
      doBangGiaoDich();
      capNhatTomTat();
      return Promise.resolve([]);
    }

    var q = el.qSearch ? (el.qSearch.value || "").trim() : "";
    var params = {
      taiKhoanId: taiKhoanId,
      q: q || undefined,
      page: 1,
      pageSize: 50,
      sort: "NgayGD_desc",
      includeDeleted: false
    };

    var qsParts = [];
    Object.keys(params).forEach(function (k) {
      var v = params[k];
      if (v === undefined || v === null || v === "") return;
      qsParts.push(encodeURIComponent(k) + "=" + encodeURIComponent(String(v)));
    });
    var qsStr = qsParts.length ? ("?" + qsParts.join("&")) : "";

    var urls = ENDPOINTS.transactions.map(function (u) { return u + qsStr; });

    return goiNhieuEndpoint(urls, { method: "GET", headers: headersJson() })
      .then(function (data) {
        var list = Array.isArray(data) ? data : (data && (data.items || data.data) ? (data.items || data.data) : []);
        state.danhSachGiaoDich = chuanHoaDanhSach(list);
        doBangGiaoDich();
        capNhatTomTat();
        return state.danhSachGiaoDich;
      })
      .catch(function (err) {
        console.error("[GiaoDich] Lỗi tải giao dịch", err);
        // giữ dữ liệu cũ, nhưng báo
        var msg = (err && err.data && err.data.message) ? err.data.message : (err.data ? JSON.stringify(err.data) : (err.message || "Không rõ lỗi"));
        alert("Lỗi tải giao dịch: " + msg);
        return [];
      });
  }

  // =========================
  // CREATE GIAO DICH
  // =========================
  function taoGiaoDich() {
    var taiKhoanId = state.taiKhoanId || Number(localStorage.getItem("taiKhoanId") || 0);

    var loaiGD = (el.loaiGD && el.loaiGD.value) ? String(el.loaiGD.value).toUpperCase() : "";
    var viId = el.viId ? Number(el.viId.value || 0) : 0;
    var danhMucId = el.danhMucId ? Number(el.danhMucId.value || 0) : 0;
    var soTien = el.soTien ? Number(el.soTien.value || 0) : 0;
    var ngayGD = el.ngayGD ? (el.ngayGD.value || "") : "";
    var ghiChu = el.ghiChu ? (el.ghiChu.value || "").trim() : "";

    if (!taiKhoanId) return alert("Chưa có taiKhoanId. Hãy đăng nhập hoặc set localStorage taiKhoanId.");
    if (!loaiGD) return alert("Vui lòng chọn Loại giao dịch (THU/CHI).");
    if (!danhMucId) return alert("Vui lòng chọn Danh mục.");
    if (!viId) return alert("Vui lòng chọn Ví.");
    if (!soTien || soTien <= 0) return alert("Số tiền phải > 0.");
    if (!ngayGD) ngayGD = homNayISO();

    // Payload chuẩn theo walletbudget-client.js gợi ý
    var payload = {
      taiKhoanId: taiKhoanId,
      viId: viId,
      danhMucId: danhMucId,
      soTien: soTien,
      loaiGD: loaiGD,
      ngayGD: ngayGD,
      ghiChu: ghiChu
    };

    var urls = ENDPOINTS.transactions.slice(); // POST base URL
    return goiNhieuEndpoint(urls, { method: "POST", headers: headersJson(), body: JSON.stringify(payload) })
      .then(function () {
        // reset + close
        if (el.soTien) el.soTien.value = "";
        if (el.ghiChu) el.ghiChu.value = "";
        hideModal(el.modalAdd);

        // reload list + summary + refresh wallets (vì số dư có thể đổi)
        return taiViVaDanhMuc()
          .then(function () { return taiDanhSachGiaoDich(); })
          .then(function () { alert("Tạo giao dịch thành công!"); });
      })
      .catch(function (err) {
        console.error("[GiaoDich] Lỗi tạo giao dịch", err);
        var msg = (err && err.data && err.data.message) ? err.data.message : (err.data ? JSON.stringify(err.data) : (err.message || "Không rõ lỗi"));
        alert("Lỗi tạo giao dịch: " + msg);
      });
  }

  // =========================
  // RENDER
  // =========================
  function doBangGiaoDich() {
    if (!el.tbody) return;

    el.tbody.innerHTML = "";

    var ds = state.danhSachGiaoDich || [];
    if (!ds.length) {
      var trEmpty = document.createElement("tr");
      trEmpty.innerHTML = '<td colspan="6" style="text-align:center; padding:14px; color:#666;">Chưa có giao dịch.</td>';
      el.tbody.appendChild(trEmpty);
      return;
    }

    // Map id -> tên
    var mapVi = {};
    (state.danhSachVi || []).forEach(function (v) {
      var id = pick(v, ["id", "Id"]);
      mapVi[String(id)] = pick(v, ["tenVi", "TenVi"]) || ("Ví " + id);
    });

    var mapDM = {};
    (state.danhSachDanhMuc || []).forEach(function (dm) {
      var id = pick(dm, ["id", "Id"]);
      mapDM[String(id)] = pick(dm, ["tenDanhMuc", "TenDanhMuc"]) || ("Danh mục " + id);
    });

    ds.forEach(function (gd) {
      var loai = String(pick(gd, ["loaiGD", "LoaiGD", "loai", "Loai"]) || "").toUpperCase();
      var soTien = Number(pick(gd, ["soTien", "SoTien"]) || 0);
      var ngay = pick(gd, ["ngayGD", "NgayGD"]) || "";
      var ghiChu = pick(gd, ["ghiChu", "GhiChu"]) || "";
      var viId = pick(gd, ["viId", "ViId"]) || 0;
      var danhMucId = pick(gd, ["danhMucId", "DanhMucId"]) || 0;

      var tenVi = mapVi[String(viId)] || ("Ví " + viId);
      var tenDM = mapDM[String(danhMucId)] || ("Danh mục " + danhMucId);

      var badgeClass = (loai === "THU") ? "badge-income" : "badge-expense";
      var amountClass = (loai === "THU") ? "amount-income" : "amount-expense";
      var sign = (loai === "THU") ? "+" : "-";

      var tr = document.createElement("tr");
      tr.innerHTML =
        "<td>" + escapeHtml(dinhDangNgay(ngay)) + "</td>" +
        '<td><span class="badge ' + badgeClass + '">' + escapeHtml(loai) + "</span></td>" +
        "<td>" + escapeHtml(tenDM) + "</td>" +
        "<td>" + escapeHtml(tenVi) + "</td>" +
        '<td class="' + amountClass + '">' + sign + fmtTien(soTien) + " ₫</td>" +
        "<td>" + escapeHtml(ghiChu) + "</td>";

      el.tbody.appendChild(tr);
    });
  }

  function capNhatTomTat() {
    var thu = 0, chi = 0;
    (state.danhSachGiaoDich || []).forEach(function (gd) {
      var loai = String(pick(gd, ["loaiGD", "LoaiGD", "loai", "Loai"]) || "").toUpperCase();
      var soTien = Number(pick(gd, ["soTien", "SoTien"]) || 0);
      if (loai === "THU") thu += soTien;
      if (loai === "CHI") chi += soTien;
    });

    if (el.sumThu) el.sumThu.textContent = "+" + fmtTien(thu) + " ₫";
    if (el.sumChi) el.sumChi.textContent = "-" + fmtTien(chi) + " ₫";
    if (el.sumLech) el.sumLech.textContent = (thu - chi >= 0 ? "+" : "-") + fmtTien(Math.abs(thu - chi)) + " ₫";
  }

  function dinhDangNgay(raw) {
    // raw có thể là "2024-12-01" hoặc DateTime ISO
    if (!raw) return "";
    var s = String(raw);
    // lấy YYYY-MM-DD
    var m = s.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (m) return m[3] + "/" + m[2] + "/" + m[1];
    return s;
  }

  // =========================
  // SELECT HELPERS
  // =========================
  function themOption(selectEl, value, text) {
    var opt = document.createElement("option");
    opt.value = value;
    opt.textContent = text;
    selectEl.appendChild(opt);
  }

  function doSelectVi(selectEl, danhSachVi, placeholder) {
    if (!selectEl) return;
    selectEl.innerHTML = "";
    themOption(selectEl, "", placeholder || "-- Chọn ví --");

    (danhSachVi || []).forEach(function (v) {
      var id = pick(v, ["id", "Id"]);
      var ten = pick(v, ["tenVi", "TenVi"]) || ("Ví " + id);
      var daXoa = !!pick(v, ["daXoa", "DaXoa"]);
      var trangThai = String(pick(v, ["trangThai", "TrangThai"]) || "");
      var isKhoa = (trangThai.toLowerCase() === "khóa" || trangThai.toLowerCase() === "khoa");

      if (!id) return;
      if (daXoa) return;
      if (isKhoa) return;

      themOption(selectEl, String(id), ten);
    });
  }

  function doDanhMucTheoLoai(selectEl, danhSachDM, loai) {
    if (!selectEl) return;
    selectEl.innerHTML = "";
    themOption(selectEl, "", "-- Danh mục --");

    var loaiUpper = String(loai || "").toUpperCase();

    (danhSachDM || []).forEach(function (dm) {
      var id = pick(dm, ["id", "Id"]);
      var ten = pick(dm, ["tenDanhMuc", "TenDanhMuc"]) || ("Danh mục " + id);
      var loaiDM = String(pick(dm, ["loai", "Loai"]) || "").toUpperCase();
      var daXoa = !!pick(dm, ["daXoa", "DaXoa"]);
      var trangThai = String(pick(dm, ["trangThai", "TrangThai"]) || "");
      var isKhoa = (trangThai.toLowerCase() === "khóa" || trangThai.toLowerCase() === "khoa");

      if (!id) return;
      if (daXoa) return;
      if (isKhoa) return;
      if (loaiUpper && loaiDM !== loaiUpper) return;

      themOption(selectEl, String(id), ten);
    });
  }

  // =========================
  // NORMALIZE LIST
  // =========================
  function chuanHoaDanhSach(ds) {
    if (!ds) return [];
    if (!Array.isArray(ds)) {
      // BE có thể trả { items: [...] }
      if (Array.isArray(ds.items)) return ds.items;
      if (Array.isArray(ds.data)) return ds.data;
      return [];
    }
    return ds;
  }
})();