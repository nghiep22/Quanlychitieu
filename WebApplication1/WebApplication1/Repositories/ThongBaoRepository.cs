using System.Data;
using System.Data.SqlClient;
using QL_NganQuy.Database;
using QL_NganQuy.Models;

namespace QL_NganQuy.Repositories
{
    public class ThongBaoRepository : IThongBaoRepository
    {
        private readonly DbConnection _dbConnection;

        public ThongBaoRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public ThongBao GetById(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM ThongBao WHERE Id = @Id";
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

        public List<ThongBao> GetAll()
        {
            var list = new List<ThongBao>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM ThongBao ORDER BY ThoiGianGui DESC";

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

        public List<ThongBao> GetByTaiKhoanId(int taiKhoanId)
        {
            var list = new List<ThongBao>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM ThongBao WHERE TaiKhoanId = @TaiKhoanId ORDER BY ThoiGianGui DESC";
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

        public List<ThongBao> GetUnreadByTaiKhoanId(int taiKhoanId)
        {
            var list = new List<ThongBao>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM ThongBao WHERE TaiKhoanId = @TaiKhoanId AND TrangThai = N'Chưa xem' ORDER BY ThoiGianGui DESC";
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

        public List<ThongBao> GetByLoai(int taiKhoanId, string loai)
        {
            var list = new List<ThongBao>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM ThongBao WHERE TaiKhoanId = @TaiKhoanId AND Loai = @Loai ORDER BY ThoiGianGui DESC";
                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@Loai", loai));

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

        public (List<ThongBao> items, int totalCount) GetPaged(int taiKhoanId, string loai, string trangThai, DateTime? tuNgay, DateTime? denNgay, int pageNumber, int pageSize)
        {
            var items = new List<ThongBao>();
            int totalCount = 0;

            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();

                // Build WHERE clause
                var whereClauses = new List<string> { "TaiKhoanId = @TaiKhoanId" };
                if (!string.IsNullOrEmpty(loai))
                    whereClauses.Add("Loai = @Loai");
                if (!string.IsNullOrEmpty(trangThai))
                    whereClauses.Add("TrangThai = @TrangThai");
                if (tuNgay.HasValue)
                    whereClauses.Add("ThoiGianGui >= @TuNgay");
                if (denNgay.HasValue)
                    whereClauses.Add("ThoiGianGui <= @DenNgay");

                var whereClause = string.Join(" AND ", whereClauses);

                // Count total
                var cmdCount = conn.CreateCommand();
                cmdCount.CommandText = $"SELECT COUNT(*) FROM ThongBao WHERE {whereClause}";
                cmdCount.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                if (!string.IsNullOrEmpty(loai))
                    cmdCount.Parameters.Add(new SqlParameter("@Loai", loai));
                if (!string.IsNullOrEmpty(trangThai))
                    cmdCount.Parameters.Add(new SqlParameter("@TrangThai", trangThai));
                if (tuNgay.HasValue)
                    cmdCount.Parameters.Add(new SqlParameter("@TuNgay", tuNgay.Value));
                if (denNgay.HasValue)
                    cmdCount.Parameters.Add(new SqlParameter("@DenNgay", denNgay.Value));

                totalCount = (int)cmdCount.ExecuteScalar();

                // Get paged data
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                    SELECT * FROM ThongBao 
                    WHERE {whereClause}
                    ORDER BY ThoiGianGui DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                if (!string.IsNullOrEmpty(loai))
                    cmd.Parameters.Add(new SqlParameter("@Loai", loai));
                if (!string.IsNullOrEmpty(trangThai))
                    cmd.Parameters.Add(new SqlParameter("@TrangThai", trangThai));
                if (tuNgay.HasValue)
                    cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay.Value));
                if (denNgay.HasValue)
                    cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay.Value));
                cmd.Parameters.Add(new SqlParameter("@Offset", (pageNumber - 1) * pageSize));
                cmd.Parameters.Add(new SqlParameter("@PageSize", pageSize));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapToModel(reader));
                    }
                }
            }

            return (items, totalCount);
        }

        public int Create(ThongBao thongBao)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO ThongBao (TaiKhoanId, TieuDe, NoiDung, Loai, GiaoDichId, NganSachId, MucTieuId, ThoiGianGui, TrangThai)
                    VALUES (@TaiKhoanId, @TieuDe, @NoiDung, @Loai, @GiaoDichId, @NganSachId, @MucTieuId, @ThoiGianGui, @TrangThai);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", thongBao.TaiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@TieuDe", thongBao.TieuDe));
                cmd.Parameters.Add(new SqlParameter("@NoiDung", thongBao.NoiDung));
                cmd.Parameters.Add(new SqlParameter("@Loai", thongBao.Loai));
                cmd.Parameters.Add(new SqlParameter("@GiaoDichId", (object)thongBao.GiaoDichId ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@NganSachId", (object)thongBao.NganSachId ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@MucTieuId", (object)thongBao.MucTieuId ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@ThoiGianGui", DateTime.Now));
                cmd.Parameters.Add(new SqlParameter("@TrangThai", "Chưa xem"));

                return (int)cmd.ExecuteScalar();
            }
        }

        public bool Update(ThongBao thongBao)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE ThongBao 
                    SET TrangThai = @TrangThai
                    WHERE Id = @Id";

                cmd.Parameters.Add(new SqlParameter("@Id", thongBao.Id));
                cmd.Parameters.Add(new SqlParameter("@TrangThai", thongBao.TrangThai));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM ThongBao WHERE Id = @Id";
                cmd.Parameters.Add(new SqlParameter("@Id", id));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool MarkAsRead(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE ThongBao SET TrangThai = N'Đã xem' WHERE Id = @Id";
                cmd.Parameters.Add(new SqlParameter("@Id", id));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool MarkAllAsRead(int taiKhoanId)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE ThongBao SET TrangThai = N'Đã xem' WHERE TaiKhoanId = @TaiKhoanId AND TrangThai = N'Chưa xem'";
                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));

                cmd.ExecuteNonQuery();
                return true;
            }
        }

        public int CountUnread(int taiKhoanId)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM ThongBao WHERE TaiKhoanId = @TaiKhoanId AND TrangThai = N'Chưa xem'";
                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));

                return (int)cmd.ExecuteScalar();
            }
        }

        public Dictionary<string, int> GetStatisticsByLoai(int taiKhoanId)
        {
            var dict = new Dictionary<string, int>();
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Loai, COUNT(*) as Count FROM ThongBao WHERE TaiKhoanId = @TaiKhoanId GROUP BY Loai";
                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dict[reader["Loai"].ToString()] = Convert.ToInt32(reader["Count"]);
                    }
                }
            }
            return dict;
        }

        public int DeleteOldNotifications(int taiKhoanId, int daysOld)
        {
            using (var conn = _dbConnection.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    DELETE FROM ThongBao 
                    WHERE TaiKhoanId = @TaiKhoanId 
                    AND ThoiGianGui < @CutoffDate 
                    AND TrangThai = N'Đã xem'";

                cmd.Parameters.Add(new SqlParameter("@TaiKhoanId", taiKhoanId));
                cmd.Parameters.Add(new SqlParameter("@CutoffDate", DateTime.Now.AddDays(-daysOld)));

                return cmd.ExecuteNonQuery();
            }
        }

        private ThongBao MapToModel(IDataReader reader)
        {
            return new ThongBao
            {
                Id = Convert.ToInt32(reader["Id"]),
                TaiKhoanId = Convert.ToInt32(reader["TaiKhoanId"]),
                TieuDe = reader["TieuDe"].ToString(),
                NoiDung = reader["NoiDung"].ToString(),
                Loai = reader["Loai"].ToString(),
                GiaoDichId = reader["GiaoDichId"] == DBNull.Value ? null : Convert.ToInt32(reader["GiaoDichId"]),
                NganSachId = reader["NganSachId"] == DBNull.Value ? null : Convert.ToInt32(reader["NganSachId"]),
                MucTieuId = reader["MucTieuId"] == DBNull.Value ? null : Convert.ToInt32(reader["MucTieuId"]),
                ThoiGianGui = Convert.ToDateTime(reader["ThoiGianGui"]),
                TrangThai = reader["TrangThai"].ToString()
            };
        }
    }
}