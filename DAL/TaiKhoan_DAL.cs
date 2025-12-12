using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class TaiKhoan_DAL
    {
        private readonly string _connectionString;

        public TaiKhoan_DAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy tài khoản theo tên đăng nhập bằng ADO.NET.
        /// </summary>
        public async Task<TaiKhoan?> GetByTenDangNhapAsync(string tenDangNhap)
        {
            const string sql = @"
                SELECT Id, TenDangNhap, MatKhau, HoTen, Quyen, IsActive
                FROM TaiKhoan
                WHERE TenDangNhap = @user AND IsActive = 1";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@user", SqlDbType.NVarChar, 100).Value = tenDangNhap;

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            int idxId = reader.GetOrdinal("Id");
            int idxTenDangNhap = reader.GetOrdinal("TenDangNhap");
            int idxMatKhau = reader.GetOrdinal("MatKhau");
            int idxHoTen = reader.GetOrdinal("HoTen");
            int idxQuyen = reader.GetOrdinal("Quyen");
            int idxIsActive = reader.GetOrdinal("IsActive");

            var tk = new TaiKhoan
            {
                Id = reader.GetInt32(idxId),
                TenDangNhap = reader.GetString(idxTenDangNhap),
                MatKhau = reader.GetString(idxMatKhau),
                HoTen = reader.IsDBNull(idxHoTen) ? null : reader.GetString(idxHoTen),
                Quyen = reader.IsDBNull(idxQuyen) ? null : reader.GetString(idxQuyen),
                IsActive = reader.GetBoolean(idxIsActive)
            };

            return tk;
        }
    }
}
