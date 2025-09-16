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
using Org.BouncyCastle.Asn1.X500;

namespace CountermeasureManagement
{
    public partial class FormUpdateSolution : Form
    {
        private static readonly object lockObj = new object();
        string connStr;
        MySQLHelper db;
        string _NO_;
        public FormUpdateSolution()
        {
            InitializeComponent();
            var config = LoadConfig("Config.ini");
            connStr = $"Server={config["IP"]};Port={config["PORT"]};Database={config["DATABASE"]};Uid={config["USER"]};Pwd={config["PASSWORD"]};";
            db = new MySQLHelper(connStr);
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
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_NO_ == "")
            {
                MessageBox.Show("Chưa có dữ liệu để update đối sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                if (CheckUpdate())
                {
                    DialogResult dlt = MessageBox.Show("Xác nhận update đối sách", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dlt == DialogResult.Yes)
                    {
                        await UpdateDoiSach();
                    }
                }              
            }
        }
        private Boolean CheckUpdate()
        {
            if (richNguyenNhan.Text == "")
            {
                MessageBox.Show("Chưa nhập nguyên nhân!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (RichDoiSach.Text == "")
            {
                MessageBox.Show("Chưa nhập đối sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void FormUpdateSolution_Load(object sender, EventArgs e)
        {
            foreach (var data in Global.dataRecords)
            {
                _NO_ = data.No;               
                break;
            }
        }
        private async Task UpdateDoiSach()
        {
            try
            {
                string query = "INSERT INTO `data_reason_solution`(`no_id`, `actual_date_completed_plan`, `reason`, `solution`,`name_update`,`time_update`)" +
                    $" VALUES ('{_NO_}','{dtime.Text}','{richNguyenNhan.Text.Trim()}','{RichDoiSach.Text.Trim()}','{Global.Name}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')";
                await db.ExecuteNonQueryAsync(query);
                MessageBox.Show("Update đối sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {

            }
        }
    }
}
