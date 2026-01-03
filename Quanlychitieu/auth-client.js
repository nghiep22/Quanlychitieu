(function () {
  "use strict";

  /* =========================
     GATEWAY BASE (DUY NHẤT)
  ========================== */
  var GATEWAY_BASE = "https://localhost:7166";

  // 👉 Ocelot route: /login/{everything}
  var LOGIN_API_BASE = GATEWAY_BASE + "/login/api";

  var STORAGE_KEYS = {
    token: "token",
    user: "currentUser"
  };

  function safeJsonParse(str) {
    try { return JSON.parse(str); } catch { return null; }
  }

  function getToken() {
    return localStorage.getItem(STORAGE_KEYS.token)
        || sessionStorage.getItem(STORAGE_KEYS.token);
  }

  function getUser() {
    return safeJsonParse(
      localStorage.getItem(STORAGE_KEYS.user)
      || sessionStorage.getItem(STORAGE_KEYS.user)
      || "null"
    );
  }

  function saveSession(token, user, remember) {
    var storage = remember ? localStorage : sessionStorage;
    storage.setItem(STORAGE_KEYS.token, token);
    storage.setItem(STORAGE_KEYS.user, JSON.stringify(user || {}));

    if (remember) {
      sessionStorage.removeItem(STORAGE_KEYS.token);
      sessionStorage.removeItem(STORAGE_KEYS.user);
    } else {
      localStorage.removeItem(STORAGE_KEYS.token);
      localStorage.removeItem(STORAGE_KEYS.user);
    }
  }

  function clearSession() {
    localStorage.removeItem(STORAGE_KEYS.token);
    localStorage.removeItem(STORAGE_KEYS.user);
    sessionStorage.removeItem(STORAGE_KEYS.token);
    sessionStorage.removeItem(STORAGE_KEYS.user);
  }

  function buildHeaders(extra) {
    var h = Object.assign({ "Content-Type": "application/json" }, extra || {});
    var token = getToken();
    if (token) h.Authorization = "Bearer " + token;
    return h;
  }

  function httpJson(url, opts) {
    opts = opts || {};
    opts.headers = buildHeaders(opts.headers);

    return fetch(url, opts).then(function (res) {
      if (!res.ok) {
        return res.json().catch(() => null).then(function (err) {
          throw new Error(err?.message || "HTTP " + res.status);
        });
      }
      return res.json();
    });
  }

  /* =========================
     AUTH FUNCTIONS
  ========================== */
  function login(tenDangNhap, matKhau, remember) {
    return httpJson(LOGIN_API_BASE + "/Auth/login", {
      method: "POST",
      body: JSON.stringify({ tenDangNhap, matKhau })
    }).then(function (data) {
      if (!data.token) throw new Error("Không nhận được token");
      saveSession(data.token, data.user, remember);
      return data;
    });
  }

  function me() {
    return httpJson(LOGIN_API_BASE + "/Auth/me", {
      method: "GET"
    });
  }

  function requireAuth() {
    if (!getToken()) {
      redirectAfterLogin();
      return false;
    }
    return true;
  }

  function redirectAfterLogin() {
    var current = location.pathname.split("/").pop() || "index.html";
    sessionStorage.setItem("returnUrl", current);
    location.href = "dangnhap.html";
  }

  /* =========================
     EXPORT
  ========================== */
  window.AUTH = {
    login,
    me,
    getToken,
    getUser,
    saveSession,
    clearSession,
    requireAuth,
    redirectAfterLogin
  };
})();
