using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CountermeasureManagement
{
    public partial class FormMain : Sunny.UI.UIForm
    {
        private static readonly object lockObj = new object();
        string connStr;
        MySQLHelper db;
        public FormMain()
        {
            InitializeComponent();
            var config = LoadConfig("Config.ini");
            connStr = $"Server={config["IP"]};Port={config["PORT"]};Database={config["DATABASE"]};Uid={config["USER"]};Pwd={config["PASSWORD"]};";
            db = new MySQLHelper(connStr);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private Dictionary<string, string> LoadConfig(string path)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                var parts = line.Split(new char[] { '=' }, 2);
                dict[parts[0].Trim()] = parts[1].Trim();
            }
            return dict;
        }
        public void WriteLog(string message)
        {
            try
            {
                string logFile = $"log{DateTime.Now.ToString("yyyyMMdd")}.txt";
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

        private async void btnLamMoi_Click(object sender, EventArgs e)
        {
            btnLamMoi.Enabled = false;
            await LoadDataAsync();
            btnLamMoi.Enabled = true;
        }
        private async Task LoadDataAsync()
        {
            try
            {
                //string query = "SELECT `no`, `date`, `status_error`, `part_name`, `area`, `ncc_c1`, `ncc_c2`," +
                //    " `pic_qc`, `image`, `content_error`, `old_error`, `new_error`, `rank`, `qty`, `qty_total`, `solution`, `action` FROM `data` " +
                //    $"where `date` between '{dtime1.Text} 00:01:00' and '{dtime2.Text} 23:59:59' order by `date` DESC";
                string query = "SELECT `no`, data.date as 'Ngày', data.status_error as 'Tình trạng lỗi', data.part_name as 'PartName'," +
                    " data.area as 'Khu vực phát sinh', data.ncc_c1 as 'NCC Cấp1', data.ncc_c2 as 'NCC Cấp2', data.pic_qc as 'PIC QC'," +
                    " `image`, data.content_error as 'Nội dung lỗi', data.old_error as 'Cũ',data.new_error as 'Mới',data.rank as 'Rank'," +
                    " data.qty as 'Qty', data.qty_total as 'Qty Total', data.solution as 'Phương án xử lý lỗi', data.action as 'Action tạm thời'," +
                    "data.plan_complete as 'Plan ht đối sách',data_reason_solution.actual_date_completed_plan as 'Thực tế ht đối sách'," +
                    "data_reason_solution.reason as 'Nguyên nhân',data_reason_solution.solution as 'Đối sách' " +
                    "FROM data LEFT JOIN data_reason_solution ON data.no = data_reason_solution.no_id " +
                    $"where data.date between '{dtime1.Text} 00:01:00' and '{dtime2.Text} 23:59:59' order by data.date DESC";
                dtg1.DataSource = await db.GetDataTableAsync(query);
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void btnThemData_Click(object sender, EventArgs e)
        {
            FormInput formInput = new FormInput();
            formInput.ShowDialog();
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {

        }
    }
}
