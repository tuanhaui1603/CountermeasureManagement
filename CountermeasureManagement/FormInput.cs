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
    public partial class FormInput : Form

    {
        private static readonly object lockObj = new object();
        string connStr;
        MySQLHelper db;
        string _NO_;
        public FormInput()
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
        private void FormInput_Load(object sender, EventArgs e)
        {
            if (Global.InsertOrDelete == "INSERT")
            {
                btnThem.Text = "Thêm";
            }
            else
            {
                btnThem.Text = "Sửa";
                foreach (var data in Global.dataRecords)
                {
                    _NO_ = data.No; 
                    dtime1.Text = data.Date;
                    cbTinhTrangLoi.Text = data.StatusError;
                    tbPartName.Text = data.PartName;
                    cbKvPhatSinh.Text = data.Area;
                    tbNccc1.Text = data.NccC1;
                    tbNccc2.Text = data.NccC2;
                    cbPicPqc.Text = data.PicQc;
                    richNoiDungLoi.Text = data.ContentError;
                    if (data.OldError == "v")
                        rd1.Checked = true;
                    if (data.NewError == "v")
                        rd2.Checked = true;
                    cbMucDoQuanTrong.Text = data.Rank;
                    numQty.Value = Int32.Parse(data.Qty);
                    cbPhuongAnXuLy.Text = data.Solution;
                    richActionTamThoi.Text = data.Action;
                    dtime2.Text = data.PlanComplete;
                    break;
                }                    
            }    
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private async void btnThem_Click(object sender, EventArgs e)
        {
            if (CheckValueInsert())
            {
                if (Global.InsertOrDelete == "INSERT")
                {
                    DialogResult dlt = MessageBox.Show("Xác nhận thêm dữ liệu", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dlt == DialogResult.Yes)
                    {
                        await InsertData();
                    }
                }
                else
                {
                    if(_NO_ == "")
                    {
                        MessageBox.Show("Chưa có dữ liệu để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }    
                    else
                    {
                        DialogResult dlt = MessageBox.Show("Xác nhận sửa dữ liệu", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dlt == DialogResult.Yes)
                        {
                            await EditData();
                        }
                    }    
                }
                
            }
        }
        private Boolean CheckValueInsert()
        {
            if (string.IsNullOrWhiteSpace(cbTinhTrangLoi.Text))
            {
                MessageBox.Show("Tình trạng lỗi không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(tbPartName.Text))
            {
                MessageBox.Show("Part name không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(cbKvPhatSinh.Text))
            {
                MessageBox.Show("Khu vực phát sinh không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(tbNccc1.Text))
            {
                MessageBox.Show("NCCC1 không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(tbNccc2.Text))
            {
                MessageBox.Show("NCCC2 không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(cbPicPqc.Text))
            {
                MessageBox.Show("PIC PQC không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(richNoiDungLoi.Text))
            {
                MessageBox.Show("Nội dung lỗi không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(cbMucDoQuanTrong.Text))
            {
                MessageBox.Show("Mức độ quan trọng không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (Int32.Parse(numQty.Value.ToString()) <= 0)
            {
                MessageBox.Show("Số lượng lỗi phải lớn hơn 0!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(cbPhuongAnXuLy.Text))
            {
                MessageBox.Show("Phương án xử lý lỗi không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(richActionTamThoi.Text))
            {
                MessageBox.Show("Action tạm thời không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        private async Task InsertData()
        {
            try
            {
                string rd11 = "", rd22 = "";
                if (rd1.Checked)
                    rd11 = "v";
                if (rd2.Checked)
                    rd22 = "v";
                string query = "INSERT INTO `data`(`date`, `status_error`, `part_name`, `area`, `ncc_c1`, `ncc_c2`, `pic_qc`, `image`, `content_error`, `old_error`," +
                    " `new_error`, `rank`, `qty`, `qty_total`, `solution`, `action`, `plan_complete`, `nguoi_nhap`, `time_nhap`) VALUES " +
                    $"('{dtime1.Text}','{cbTinhTrangLoi.Text}','{tbPartName.Text}','{cbKvPhatSinh.Text}','{tbNccc1.Text}','{tbNccc2.Text}','{cbPicPqc.Text}'" +
                    $",'','{richNoiDungLoi.Text}','{rd11}','{rd22}','{cbMucDoQuanTrong.Text}','{numQty.Value}',''," +
                    $"'{cbPhuongAnXuLy.Text}','{richActionTamThoi.Text}','{dtime2.Text}','{Global.Name}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')";
                await db.ExecuteNonQueryAsync(query);
                MessageBox.Show("Thêm dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task EditData()
        {
            try
            {
                string rd11 = "", rd22 = "";
                if (rd1.Checked)
                    rd11 = "v";
                if (rd2.Checked)
                    rd22 = "v";
                string query = $"UPDATE `data` SET `date`='{dtime1.Text}',`status_error`='{cbTinhTrangLoi.Text.Trim()}',`part_name`='{tbPartName.Text.Trim()}',`area`='{cbKvPhatSinh.Text.Trim()}'" +
                    $",`ncc_c1`='{tbNccc1.Text.Trim()}',`ncc_c2`='{tbNccc2.Text.Trim()}'," +
                    $"`pic_qc`='{cbPicPqc.Text.Trim()}',`image`='',`content_error`='{richNoiDungLoi.Text.Trim()}',`old_error`='{rd11}',`new_error`='{rd22}'," +
                    $"`rank`='{cbMucDoQuanTrong.Text.Trim()}',`qty`='{numQty.Text.Trim()}'," +
                    $"`solution`='{cbPhuongAnXuLy.Text.Trim()}',`action`='{richActionTamThoi.Text.Trim()}',`plan_complete`='{dtime2.Text.Trim()}'" +
                    $" WHERE `no` = '{_NO_}'";
                await db.ExecuteNonQueryAsync(query);
                MessageBox.Show("Sửa dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
