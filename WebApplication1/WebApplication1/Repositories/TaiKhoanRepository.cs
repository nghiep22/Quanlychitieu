using System.Data;
using System.Data.SqlClient;
using QL_NganQuy.Database;
using QL_NganQuy.Models;

namespace QL_NganQuy.Repositories
{
    public class TaiKhoanRepository : ITaiKhoanRepository
    {
        private readonly DbConnection _dbConnection;

        public TaiKhoanRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public TaiKhoan GetById(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM TaiKhoan WHERE Id = @Id";
                cmd.Parameters.Add(new SqlParameter("@Id", id));

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapToModel(reader);
                    }
                }
            }
            return null;
        }

        public List<TaiKhoan> GetAll()
        {
            var list = new List<TaiKhoan>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM TaiKhoan ORDER BY Id DESC";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapToModel(reader));
                    }
                }
            }
            return list;
        }

        public List<TaiKhoan> Search(string quyen, bool? isActive)
        {
            var list = new List<TaiKhoan>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                var whereClauses = new List<string>();
                if (!string.IsNullOrEmpty(quyen))
                    whereClauses.Add("Quyen = @Quyen");
                if (isActive.HasValue)
                    whereClauses.Add("IsActive = @IsActive");

                var whereClause = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";
                cmd.CommandText = $"SELECT * FROM TaiKhoan {whereClause} ORDER BY Id DESC";

                if (!string.IsNullOrEmpty(quyen))
                    cmd.Parameters.Add(new SqlParameter("@Quyen", quyen));
                if (isActive.HasValue)
                    cmd.Parameters.Add(new SqlParameter("@IsActive", isActive.Value));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapToModel(reader));
                    }
                }
            }
            return list;
        }

        public int Create(TaiKhoan taiKhoan)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, HoTen, Quyen, IsActive)
                    VALUES (@TenDangNhap, @MatKhau, @HoTen, @Quyen, @IsActive);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                cmd.Parameters.Add(new SqlParameter("@TenDangNhap", taiKhoan.TenDangNhap));
                cmd.Parameters.Add(new SqlParameter("@MatKhau", taiKhoan.MatKhau));
                cmd.Parameters.Add(new SqlParameter("@HoTen", taiKhoan.HoTen ?? ""));
                cmd.Parameters.Add(new SqlParameter("@Quyen", taiKhoan.Quyen ?? "User"));
                cmd.Parameters.Add(new SqlParameter("@IsActive", taiKhoan.IsActive));

                return (int)cmd.ExecuteScalar();
            }
        }

        public bool Update(TaiKhoan taiKhoan)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE TaiKhoan 
                    SET HoTen = @HoTen, 
                        Quyen = @Quyen,
                        IsActive = @IsActive
                    WHERE Id = @Id";

                cmd.Parameters.Add(new SqlParameter("@Id", taiKhoan.Id));
                cmd.Parameters.Add(new SqlParameter("@HoTen", taiKhoan.HoTen ?? ""));
                cmd.Parameters.Add(new SqlParameter("@Quyen", taiKhoan.Quyen ?? "User"));
                cmd.Parameters.Add(new SqlParameter("@IsActive", taiKhoan.IsActive));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM TaiKhoan WHERE Id = @Id";
                cmd.Parameters.Add(new SqlParameter("@Id", id));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool CheckTenDangNhapExists(string tenDangNhap, int? excludeId)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (excludeId.HasValue)
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap AND Id != @ExcludeId";
                    cmd.Parameters.Add(new SqlParameter("@ExcludeId", excludeId.Value));
                }
                else
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap";
                }

                cmd.Parameters.Add(new SqlParameter("@TenDangNhap", tenDangNhap));

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private TaiKhoan MapToModel(IDataReader reader)
        {
            return new TaiKhoan
            {
                Id = Convert.ToInt32(reader["Id"]),
                TenDangNhap = reader["TenDangNhap"].ToString(),
                MatKhau = reader["MatKhau"].ToString(),
                HoTen = reader["HoTen"] == DBNull.Value ? "" : reader["HoTen"].ToString(),
                Quyen = reader["Quyen"] == DBNull.Value ? "User" : reader["Quyen"].ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }
    }
}