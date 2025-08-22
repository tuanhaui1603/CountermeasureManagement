using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CountermeasureManagement
{
    public class MySQLHelper : IDisposable
    {
        private static readonly object lockObj = new object();
        private MySqlConnection conn;

        public MySQLHelper(string connectionString)
        {
            conn = new MySqlConnection(connectionString);
        }

        public void Open()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        public void Close()
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                Open();
                using (MySqlDataAdapter da = new MySqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi GetDataTable: " + ex.Message);
            }
            return dt;
        }

        public int ExecuteNonQuery(string sql)
        {
            try
            {
                Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi ExecuteNonQuery: " + ex.Message);
                return -1;
            }
        }

        public object ExecuteScalar(string sql)
        {
            try
            {
                Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi ExecuteScalar: " + ex.Message);
                return null;
            }
        }

        public void Dispose()
        {
            Close();
            conn.Dispose();
        }
        public void WriteLog(string message)
        {
            try
            {
                string logFile = $"log{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
                lock (lockObj) // tránh nhiều thread ghi cùng lúc bị lỗi
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFile);
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                    File.AppendAllText(logPath, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
               
            }
        }
    }
}
