(function () {
  "use strict";

  // ======================================================
  // CONFIG – API GATEWAY (THEO SWAGGER)
  // ======================================================
  // Swagger: https://localhost:7166/swagger
  // Base:    https://localhost:7166/walletbudget/api
  var API_BASE = (window.__WALLETBUDGET_API_BASE ||
    "https://localhost:7166/walletbudget/api").replace(/\/+$/, "");

  var ENDPOINTS = {
    wallets: [API_BASE + "/wallets", API_BASE + "/vi"],
    categories: [API_BASE + "/categories", API_BASE + "/danhmuc"],
    transactions: [API_BASE + "/transactions", API_BASE + "/giaodich"]
  };

  // ======================================================
  // CACHE
  // ======================================================
  var CACHE_VI = "cache_danhSachVi";
  var CACHE_DM = "cache_danhSachDanhMuc";

  // ======================================================
  // AUTH
  // ======================================================
  function layTaiKhoanId() {
    try {
      if (window.AUTH && typeof window.AUTH.getUser === "function") {
        var u = window.AUTH.getUser();
        return Number(u?.id || u?.Id || u?.taiKhoanId || u?.TaiKhoanId || 0);
      }
    } catch {}
    return Number(localStorage.getItem("taiKhoanId") || 0);
  }

  function layToken() {
    try {
      if (window.AUTH && typeof window.AUTH.getToken === "function") {
        return window.AUTH.getToken();
      }
    } catch {}
    return localStorage.getItem("token") || "";
  }

  function headersJson() {
    var h = { "Content-Type": "application/json" };
    var token = layToken();
    if (token) h.Authorization = "Bearer " + token;
    return h;
  }

  // ======================================================
  // HELPERS
  // ======================================================
  function qs(s, r) { return (r || document).querySelector(s); }
  function qsa(s, r) { return Array.from((r || document).querySelectorAll(s)); }
  function byId(id) { return document.getElementById(id); }

  function homNay() {
    return new Date().toISOString().slice(0, 10);
  }

  function fmtTien(n) {
    return Number(n || 0).toLocaleString("vi-VN") + " ₫";
  }

  function fetchJson(url, opt) {
    return fetch(url, opt).then(r =>
      r.text().then(t => {
        var d = t ? JSON.parse(t) : null;
        if (!r.ok) throw d || r.statusText;
        return d;
      })
    );
  }

  function goiNhieuEndpoint(urls, opt) {
    var p = Promise.reject();
    var last;
    urls.forEach(u => {
      p = p.catch(() => fetchJson(u, opt).catch(e => { last = e; throw e; }));
    });
    return p.catch(() => { throw last; });
  }

  // ======================================================
  // STATE
  // ======================================================
  var state = {
    taiKhoanId: 0,
    vi: [],
    danhMuc: [],
    giaoDich: []
  };

  // ======================================================
  // ELEMENTS
  // ======================================================
  var el = {
    tbody: qs("table tbody"),
    btnThem: byId("btnAddTransaction"),
    modal: byId("modalAddTransaction"),
    loai: byId("transactionType"),
    vi: byId("walletId"),
    dm: byId("categoryId"),
    tien: byId("soTien"),
    ngay: byId("ngayGD"),
    ghiChu: byId("ghiChu"),
    form: qs("#modalAddTransaction form"),
    sumThu: qs(".stat-income"),
    sumChi: qs(".stat-expense"),
    sumLech: qs(".stat-balance")
  };

  // ======================================================
  // INIT
  // ======================================================
  document.addEventListener("DOMContentLoaded", init);
  function init() {
    state.taiKhoanId = layTaiKhoanId();
    el.ngay.value = homNay();

    ganSuKien();
    taiDuLieu();
  }

  function ganSuKien() {
    el.btnThem?.addEventListener("click", () => el.modal.style.display = "flex");
    el.form?.addEventListener("submit", e => {
      e.preventDefault();
      taoGiaoDich();
    });
    el.loai?.addEventListener("change", locDanhMuc);
  }

  // ======================================================
  // LOAD DATA
  // ======================================================
  function taiDuLieu() {
    Promise.all([taiVi(), taiDanhMuc()])
      .then(taiGiaoDich)
      .catch(console.error);
  }

  function taiVi() {
    return goiNhieuEndpoint(
      ENDPOINTS.wallets.map(u => `${u}?taiKhoanId=${state.taiKhoanId}`),
      { headers: headersJson() }
    ).then(d => {
      state.vi = d || [];
      renderSelect(el.vi, state.vi, "tenVi");
    });
  }

  function taiDanhMuc() {
    return goiNhieuEndpoint(
      ENDPOINTS.categories.map(u => `${u}?taiKhoanId=${state.taiKhoanId}`),
      { headers: headersJson() }
    ).then(d => {
      state.danhMuc = d || [];
      locDanhMuc();
    });
  }

  function taiGiaoDich() {
    return goiNhieuEndpoint(
      ENDPOINTS.transactions.map(u => `${u}?taiKhoanId=${state.taiKhoanId}`),
      { headers: headersJson() }
    ).then(d => {
      state.giaoDich = d || [];
      renderBang();
      capNhatTong();
    });
  }

  // ======================================================
  // CREATE
  // ======================================================
  function taoGiaoDich() {
    var payload = {
      taiKhoanId: state.taiKhoanId,
      viId: Number(el.vi.value),
      danhMucId: Number(el.dm.value),
      soTien: Number(el.tien.value),
      loaiGD: el.loai.value,
      ngayGD: el.ngay.value,
      ghiChu: el.ghiChu.value
    };

    goiNhieuEndpoint(ENDPOINTS.transactions, {
      method: "POST",
      headers: headersJson(),
      body: JSON.stringify(payload)
    }).then(() => {
      alert("✅ Thêm giao dịch thành công");
      el.modal.style.display = "none";
      taiDuLieu();
    }).catch(e => alert("❌ Lỗi: " + JSON.stringify(e)));
  }

  // ======================================================
  // RENDER
  // ======================================================
  function renderBang() {
    el.tbody.innerHTML = "";
    state.giaoDich.forEach(g => {
      var tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${g.ngayGD?.slice(0, 10)}</td>
        <td>${g.loaiGD}</td>
        <td>${g.tenDanhMuc || ""}</td>
        <td>${g.tenVi || ""}</td>
        <td>${fmtTien(g.soTien)}</td>
        <td>${g.ghiChu || ""}</td>`;
      el.tbody.appendChild(tr);
    });
  }

  function capNhatTong() {
    var thu = 0, chi = 0;
    state.giaoDich.forEach(g => {
      g.loaiGD === "THU" ? thu += g.soTien : chi += g.soTien;
    });
    el.sumThu.textContent = fmtTien(thu);
    el.sumChi.textContent = fmtTien(chi);
    el.sumLech.textContent = fmtTien(thu - chi);
  }

  function renderSelect(sel, ds, field) {
    sel.innerHTML = `<option value="">-- chọn --</option>`;
    ds.forEach(x => {
      sel.innerHTML += `<option value="${x.id}">${x[field]}</option>`;
    });
  }

  function locDanhMuc() {
    var loai = el.loai.value;
    renderSelect(
      el.dm,
      state.danhMuc.filter(d => !loai || d.loai === loai),
      "tenDanhMuc"
    );
  }
})();
