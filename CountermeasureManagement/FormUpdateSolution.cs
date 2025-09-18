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
                string _4m1_con_nguoi = "0", _4m1_phuong_phap="0", _4m1_may_moc = "0", _4m1_vat_lieu = "0";
                string _4m2_con_nguoi = "0", _4m2_phuong_phap = "0", _4m2_may_moc = "0", _4m2_vat_lieu = "0";
                if(rd1_ConNguoi.Checked)
                    _4m1_con_nguoi = "1";
                if (rd1_PhuongPhap.Checked)
                    _4m1_phuong_phap = "1";
                if (rd1_MayMoc.Checked)
                    _4m1_may_moc = "1";
                if (rd1_VatLieu.Checked)
                    _4m1_vat_lieu = "1";
                if (rd2_ConNguoi.Checked)
                    _4m2_con_nguoi = "1";
                if (rd2_PhuongPhap.Checked)
                    _4m2_phuong_phap = "1";
                if (rd2_MayMoc.Checked)
                    _4m2_may_moc = "1";
                if (rd2_VatLieu.Checked)
                    _4m2_vat_lieu = "1";
                string query_4m_nguyenNhan = "INSERT INTO `reason`(`no_id`, `con_nguoi`, `phuong_phap`, `may_moc`, `vat_lieu`) " +
                    $"VALUES ('{_NO_}','{_4m1_con_nguoi}','{_4m1_phuong_phap}','{_4m1_may_moc}','{_4m1_vat_lieu}')";
                string query_4m_doiSach = "INSERT INTO `method`(`no_id`, `con_nguoi`, `phuong_phap`, `may_moc`, `vat_lieu`) " +
                    $"VALUES ('{_NO_}','{_4m2_con_nguoi}','{_4m2_phuong_phap}','{_4m2_may_moc}','{_4m2_vat_lieu}')";
                await db.ExecuteNonQueryAsync(query);
                await db.ExecuteNonQueryAsync(query_4m_nguyenNhan);
                await db.ExecuteNonQueryAsync(query_4m_doiSach);
                if (Global.CheckExecuteQueryMySql)
                {
                    MessageBox.Show("Update đối sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Lỗi kết nối đến server để update đối sách: " + Global.MessageErrorExecuteQueryMySql, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }             
            }
            catch
            {

            }
        }
    }
}
