using System.Data;
using System.Data.SqlClient;
using QL_NganQuy.Database;
using QL_NganQuy.Models;

namespace QL_NganQuy.Repositories
{
    public class ViRepository : IViRepository
    {
        private readonly DbConnection _dbConnection;

        public ViRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public Vi GetById(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Vi WHERE Id = @Id AND DaXoa = 0";
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

        public List<Vi> GetAll()
        {
            var list = new List<Vi>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Vi WHERE DaXoa = 0 ORDER BY NgayTao DESC";

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

        public List<Vi> GetByTaiKhoanId(int taiKhoanId)
        {
            var list = new List<Vi>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Vi WHERE TaiKhoanId = @TaiKhoanId AND DaXoa = 0 ORDER BY NgayTao DESC";
                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));

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

        public List<Vi> Search(int taiKhoanId, string loaiVi, string trangThai)
        {
            var list = new List<Vi>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                var whereClauses = new List<string> { "TaiKhoanId = @TaiKhoanId", "DaXoa = 0" };
                if (!string.IsNullOrEmpty(loaiVi))
                    whereClauses.Add("LoaiVi = @LoaiVi");
                if (!string.IsNullOrEmpty(trangThai))
                    whereClauses.Add("TrangThai = @TrangThai");

                var whereClause = string.Join(" AND ", whereClauses);
                cmd.CommandText = $"SELECT * FROM Vi WHERE {whereClause} ORDER BY NgayTao DESC";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                if (!string.IsNullOrEmpty(loaiVi))
                    cmd.Parameters.Add(new SqlParameter("@LoaiVi", loaiVi));
                if (!string.IsNullOrEmpty(trangThai))
                    cmd.Parameters.Add(new SqlParameter("@TrangThai", trangThai));

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

        public int Create(Vi vi)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Vi (TaiKhoanId, TenVi, LoaiVi, SoDuBanDau, GhiChu, TrangThai, DaXoa, NgayTao, NgayCapNhat)
                    VALUES (@TaiKhoanId, @TenVi, @LoaiVi, @SoDuBanDau, @GhiChu, @TrangThai, @DaXoa, @NgayTao, @NgayCapNhat);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", vi.TaiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TenVi", vi.TenVi));
                cmd.Parameters.Add(new SqlParameter("@LoaiVi", vi.LoaiVi));
                cmd.Parameters.Add(new SqlParameter("@SoDuBanDau", vi.SoDuBanDau));
                cmd.Parameters.Add(new SqlParameter("@GhiChu", vi.GhiChu ?? ""));
                cmd.Parameters.Add(new SqlParameter("@TrangThai", "Hoạt động"));
                cmd.Parameters.Add(new SqlParameter("@DaXoa", false));
                cmd.Parameters.Add(new SqlParameter("@NgayTao", DateTime.Now));
                cmd.Parameters.Add(new SqlParameter("@NgayCapNhat", DateTime.Now));

                return (int)cmd.ExecuteScalar();
            }
        }

        public bool Update(Vi vi)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE Vi 
                    SET TenVi = @TenVi, 
                        LoaiVi = @LoaiVi,
                        GhiChu = @GhiChu,
                        TrangThai = @TrangThai,
                        NgayCapNhat = @NgayCapNhat
                    WHERE Id = @Id";

                cmd.Parameters.Add(new SqlParameter("@Id", vi.Id));
                cmd.Parameters.Add(new SqlParameter("@TenVi", vi.TenVi));
                cmd.Parameters.Add(new SqlParameter("@LoaiVi", vi.LoaiVi));
                cmd.Parameters.Add(new SqlParameter("@GhiChu", vi.GhiChu ?? ""));
                cmd.Parameters.Add(new SqlParameter("@TrangThai", vi.TrangThai));
                cmd.Parameters.Add(new SqlParameter("@NgayCapNhat", DateTime.Now));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Vi SET DaXoa = 1 WHERE Id = @Id";
                cmd.Parameters.Add(new SqlParameter("@Id", id));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool CheckTenViExists(int taiKhoanId, string tenVi, int? excludeId)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (excludeId.HasValue)
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Vi WHERE TaiKhoanId = @TaiKhoanId AND TenVi = @TenVi AND Id != @ExcludeId AND DaXoa = 0";
                    cmd.Parameters.Add(new SqlParameter("@ExcludeId", excludeId.Value));
                }
                else
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Vi WHERE TaiKhoanId = @TaiKhoanId AND TenVi = @TenVi AND DaXoa = 0";
                }

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TenVi", tenVi));

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private Vi MapToModel(IDataReader reader)
        {
            return new Vi
            {
                Id = Convert.ToInt32(reader["Id"]),
                TaiKhoanId = Convert.ToInt32(reader["TaiKhoanId"]),
                TenVi = reader["TenVi"].ToString(),
                LoaiVi = reader["LoaiVi"].ToString(),
                SoDuBanDau = Convert.ToDecimal(reader["SoDuBanDau"]),
                GhiChu = reader["GhiChu"] == DBNull.Value ? "" : reader["GhiChu"].ToString(),
                TrangThai = reader["TrangThai"].ToString(),
                DaXoa = Convert.ToBoolean(reader["DaXoa"]),
                NgayTao = Convert.ToDateTime(reader["NgayTao"]),
                NgayCapNhat = Convert.ToDateTime(reader["NgayCapNhat"])
            };
        }
    }
}