(function () {
  "use strict";

  // ✅ GỌI QUA API GATEWAY (Ocelot)
  // Gateway base: http://localhost:7001
  // Route Login trên Gateway: /login/{everything}
  // => API base: http://localhost:7001/login/api
  var LOGIN_API_BASE = "http://localhost:7001/login/api";

  var STORAGE_KEYS = {
    token: "token",
    user: "currentUser"
  };

  function safeJsonParse(str) {
    try { return JSON.parse(str); } catch { return null; }
  }

  function getToken() {
    return localStorage.getItem(STORAGE_KEYS.token) || sessionStorage.getItem(STORAGE_KEYS.token);
  }

  function getUser() {
    var raw = localStorage.getItem(STORAGE_KEYS.user) || sessionStorage.getItem(STORAGE_KEYS.user);
    return safeJsonParse(raw);
  }

  function saveSession(token, user, remember) {
    // remember=true -> localStorage, false -> sessionStorage
    clearSession();
    var store = remember ? localStorage : sessionStorage;
    store.setItem(STORAGE_KEYS.token, token);
    store.setItem(STORAGE_KEYS.user, JSON.stringify(user || null));
  }

  function clearSession() {
    localStorage.removeItem(STORAGE_KEYS.token);
    localStorage.removeItem(STORAGE_KEYS.user);
    sessionStorage.removeItem(STORAGE_KEYS.token);
    sessionStorage.removeItem(STORAGE_KEYS.user);
  }

  async function fetchJson(url, options) {
    options = options || {};
    options.headers = options.headers || {};
    options.headers["Content-Type"] = "application/json";

    var token = getToken();
    if (token) options.headers["Authorization"] = "Bearer " + token;

    var res = await fetch(url, options);
    var text = await res.text();
    var data = null;
    try { data = text ? JSON.parse(text) : null; } catch { data = text; }

    if (!res.ok) {
      var msg = (data && data.message) ? data.message : ("HTTP " + res.status);
      var err = new Error(msg);
      err.status = res.status;
      err.data = data;
      throw err;
    }
    return data;
  }

  async function login(tenDangNhap, matKhau, remember) {
    var url = LOGIN_API_BASE + "/auth/login";
    var payload = { tenDangNhap: tenDangNhap, matKhau: matKhau };
    var data = await fetchJson(url, { method: "POST", body: JSON.stringify(payload) });

    // Backend trả: { token, user: {id, tenDangNhap, hoTen, quyen} }
    if (!data || !data.token) throw new Error("Không nhận được token từ server.");
    saveSession(data.token, data.user, remember);
    return data;
  }

  // ✅ Đăng ký: thử nhiều endpoint phổ biến (tuỳ BE của bạn)
  async function register(hoTen, tenDangNhap, matKhau) {
    var candidates = [
      LOGIN_API_BASE + "/auth/register",      // nếu bạn có AuthController Register
      LOGIN_API_BASE + "/taikhoan/register",  // nếu có TaiKhoanController register
      LOGIN_API_BASE + "/taikhoan"            // nếu POST tạo tài khoản
    ];

    var payload = { hoTen: hoTen, tenDangNhap: tenDangNhap, matKhau: matKhau };

    var lastErr = null;
    for (var i = 0; i < candidates.length; i++) {
      try {
        return await fetchJson(candidates[i], { method: "POST", body: JSON.stringify(payload) });
      } catch (e) {
        // Nếu 404 thì thử endpoint tiếp theo, còn lỗi khác thì dừng
        lastErr = e;
        if (e && e.status === 404) continue;
        throw e;
      }
    }
    throw lastErr || new Error("Không tìm thấy endpoint đăng ký phù hợp trên BE.");
  }

  async function me() {
    return await fetchJson(LOGIN_API_BASE + "/auth/me", { method: "GET" });
  }

  function requireAuth(redirectTo) {
    var token = getToken();
    if (!token) {
      window.location.href = redirectTo || "dangnhap.html";
      return false;
    }
    return true;
  }

  function requireRole(role, redirectTo) {
    if (!requireAuth(redirectTo)) return false;

    var user = getUser();
    var q = (user && (user.Quyen || user.quyen)) ? (user.Quyen || user.quyen) : "";
    if ((q || "").toLowerCase() !== (role || "").toLowerCase()) {
      window.location.href = redirectTo || "index.html";
      return false;
    }
    return true;
  }

  function redirectAfterLogin(user) {
    var role = (user && (user.Quyen || user.quyen)) ? (user.Quyen || user.quyen) : "";
    if ((role || "").toLowerCase() === "admin") return "admin.html";
    // user thường -> trang user (mặc định index.html)
    return "index.html";
  }

  // expose global
  window.AUTH = {
    LOGIN_API_BASE: LOGIN_API_BASE,
    getToken: getToken,
    getUser: getUser,
    saveSession: saveSession,
    clearSession: clearSession,
    login: login,
    register: register,
    me: me,
    requireAuth: requireAuth,
    requireRole: requireRole,
    redirectAfterLogin: redirectAfterLogin
  };
})();