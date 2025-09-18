using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CountermeasureManagement
{
    //public class MySQLHelper : IDisposable
    //{
    //    private static readonly object lockObj = new object();

    //    private MySqlConnection conn;

    //    public MySQLHelper(string connectionString)
    //    {
    //        conn = new MySqlConnection(connectionString);
    //    }

    //    public void Open()
    //    {
    //        if (conn.State == ConnectionState.Closed)
    //            conn.Open();
    //    }

    //    public void Close()
    //    {
    //        if (conn.State == ConnectionState.Open)
    //            conn.Close();
    //    }

    //    public DataTable GetDataTable(string sql)
    //    {
    //        DataTable dt = new DataTable();
    //        try
    //        {
    //            Open();
    //            using (MySqlDataAdapter da = new MySqlDataAdapter(sql, conn))
    //            {
    //                da.Fill(dt);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            WriteLog("Lỗi GetDataTable: " + ex.Message);
    //        }
    //        return dt;
    //    }

    //    public int ExecuteNonQuery(string sql)
    //    {
    //        try
    //        {
    //            Open();
    //            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
    //            {
    //                return cmd.ExecuteNonQuery();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            WriteLog("Lỗi ExecuteNonQuery: " + ex.Message);
    //            return -1;
    //        }
    //    }

    //    public object ExecuteScalar(string sql)
    //    {
    //        try
    //        {
    //            Open();
    //            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
    //            {
    //                return cmd.ExecuteScalar();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            WriteLog("Lỗi ExecuteScalar: " + ex.Message);
    //            return null;
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        Close();
    //        conn.Dispose();
    //    }
    //    public void WriteLog(string message)
    //    {
    //        try
    //        {
    //            string logFile = $"log{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
    //            lock (lockObj) // tránh nhiều thread ghi cùng lúc bị lỗi
    //            {
    //                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFile);
    //                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
    //                File.AppendAllText(logPath, logMessage + Environment.NewLine);
    //            }
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //    }
    //}
    public class MySQLHelper : IDisposable
    {
        // Sử dụng SemaphoreSlim thay cho lock để hỗ trợ bất đồng bộ
        private static readonly SemaphoreSlim _logSemaphore = new SemaphoreSlim(1, 1);
        private MySqlConnection _conn;

        public MySQLHelper(string connectionString)
        {
            _conn = new MySqlConnection(connectionString);
        }

        // Mở kết nối bất đồng bộ
        public async Task OpenAsync()
        {
            if (_conn.State == ConnectionState.Closed)
            {
                await _conn.OpenAsync();
            }
        }

        // Đóng kết nối (phiên bản đồng bộ vì CloseAsync() không phổ biến ở framework cũ)
        public void Close()
        {
            if (_conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }

        // Lấy DataTable bất đồng bộ
        public async Task<DataTable> GetDataTableAsync(string sql)
        {
            var dt = new DataTable();
            try
            {
                await OpenAsync();
                // Sử dụng using thông thường
                using (var cmd = new MySqlCommand(sql, _conn))
                {
                    // Vẫn await việc thực thi reader
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Global.CheckExecuteQueryMySql = true;
                        dt.Load(reader);
                       
                    }
                }
            }
            catch (Exception ex)
            {
                Global.CheckExecuteQueryMySql = false;
                Global.MessageErrorExecuteQueryMySql = ex.ToString();
                await WriteLogAsync("Lỗi GetDataTableAsync: " + ex.ToString());
            }
            return dt;
        }

        // Thực thi câu lệnh không trả về kết quả bất đồng bộ
        public async Task<int> ExecuteNonQueryAsync(string sql)
        {
            try
            {
                await OpenAsync();
                using (var cmd = new MySqlCommand(sql, _conn))
                {
                    Global.CheckExecuteQueryMySql = true;
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Global.CheckExecuteQueryMySql = false;
                Global.MessageErrorExecuteQueryMySql = ex.ToString();
                await WriteLogAsync("Lỗi ExecuteNonQueryAsync: " + ex.ToString());
                return -1;
            }
        }

        // Thực thi câu lệnh trả về một giá trị duy nhất bất đồng bộ
        public async Task<object> ExecuteScalarAsync(string sql)
        {
            try
            {
                await OpenAsync();
                using (var cmd = new MySqlCommand(sql, _conn))
                {
                    return await cmd.ExecuteScalarAsync();
                }
            }
            catch (Exception ex)
            {
                await WriteLogAsync("Lỗi ExecuteScalarAsync: " + ex.ToString());
                return null;
            }
        }

        // Ghi log bất đồng bộ (sử dụng StreamWriter để tương thích tốt hơn)
        public async Task WriteLogAsync(string message)
        {
            await _logSemaphore.WaitAsync();
            try
            {
                string logFile = $"log{DateTime.Now:yyyyMMdd}.txt";
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFile);
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

                // Sử dụng StreamWriter để ghi file bất đồng bộ
                using (StreamWriter writer = new StreamWriter(logPath, true)) // true để ghi nối vào file
                {
                    await writer.WriteLineAsync(logMessage);
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi khi ghi log
            }
            finally
            {
                _logSemaphore.Release();
            }
        }

        // Chỉ implement IDisposable
        public void Dispose()
        {
            Close();
            if (_conn != null)
            {
                _conn.Dispose();
                _conn = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
