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
        string _NO_;
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
                _NO_ = "";
                string query = "SELECT `no`, data.date as 'Ngày', data.status_error as 'Tình trạng lỗi', data.part_name as 'PartName'," +
                    " data.area as 'Khu vực phát sinh', data.ncc_c1 as 'NCC Cấp1', data.ncc_c2 as 'NCC Cấp2', data.pic_qc as 'PIC QC'," +
                    " `image`, data.content_error as 'Nội dung lỗi', data.old_error as 'Cũ',data.new_error as 'Mới',data.rank as 'Rank'," +
                    " data.qty as 'Qty', data.qty_total as 'Qty Total', data.solution as 'Phương án xử lý lỗi', data.action as 'Action tạm thời'," +
                    "data.plan_complete as 'Plan ht đối sách',data_reason_solution.actual_date_completed_plan as 'Thực tế ht đối sách'," +
                    "data_reason_solution.reason as 'Nguyên nhân',data_reason_solution.solution as 'Đối sách' " +
                    "FROM data LEFT JOIN data_reason_solution ON data.no = data_reason_solution.no_id " +
                    $"where data.date between '{dtime1.Text} 00:00:01' and '{dtime2.Text} 23:59:59' order by data.date DESC";
                dtg1.DataSource = await db.GetDataTableAsync(query);
                dtg1.Columns[0].DefaultCellStyle.ForeColor = Color.Blue;
                dtg1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dtg1.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dtg1.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dtg1.Columns[9].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dtg1.Columns[16].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dtg1.Columns[19].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void btnThemData_Click(object sender, EventArgs e)
        {
            Global.InsertOrDelete = "INSERT";
            FormInput formInput = new FormInput();
            formInput.ShowDialog();
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {

        }

        private async void btnXoa_Click(object sender, EventArgs e)
        {
            if (_NO_ != "" && _NO_ != null)
            {
                DialogResult dlt = MessageBox.Show("Xác nhận xóa dữ liệu", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dlt == DialogResult.Yes)
                {
                    
                    try
                    {
                        string query1 = $"DELETE FROM data WHERE no = '{_NO_}'";
                        await db.ExecuteNonQueryAsync(query1);
                        MessageBox.Show("Xóa dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error: "+ex.ToString() , "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                   
                }

            }
            else
            {
                MessageBox.Show("Chưa chọn dữ liệu cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }    
        }

        private void dtg1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0)
                {
                    _NO_ = dtg1.Rows[e.RowIndex].Cells["no"].Value.ToString();
                    for (int i = 0; i < dtg1.Rows.Count; i++)
                    {
                        dtg1.Rows[i].DefaultCellStyle.BackColor = Color.White;

                    }
                    dtg1.CurrentRow.DefaultCellStyle.BackColor = Color.Gray;
                }
            }
            catch
            {

            }
              
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if(_NO_ != "" && _NO_ != null)
            {
                Global.InsertOrDelete = "DELETE";
                Global.dataRecords = new List<DataRecord>();
                DataRecord record = new DataRecord();
                record.No = dtg1.CurrentRow.Cells[0].Value.ToString();
                record.Date = dtg1.CurrentRow.Cells[1].Value.ToString();
                record.StatusError = dtg1.CurrentRow.Cells[2].Value.ToString();
                record.PartName = dtg1.CurrentRow.Cells[3].Value.ToString();
                record.Area = dtg1.CurrentRow.Cells[4].Value.ToString();
                record.NccC1 = dtg1.CurrentRow.Cells[5].Value.ToString();
                record.NccC2 = dtg1.CurrentRow.Cells[6].Value.ToString();
                record.PicQc = dtg1.CurrentRow.Cells[7].Value.ToString();
                record.Image = dtg1.CurrentRow.Cells[8].Value.ToString();
                record.ContentError = dtg1.CurrentRow.Cells[9].Value.ToString();
                record.OldError = dtg1.CurrentRow.Cells[10].Value.ToString();
                record.NewError = dtg1.CurrentRow.Cells[11].Value.ToString();
                record.Rank = dtg1.CurrentRow.Cells[12].Value.ToString();
                record.Qty = dtg1.CurrentRow.Cells[13].Value.ToString();
                record.QtyTotal = dtg1.CurrentRow.Cells[14].Value.ToString();
                record.Solution = dtg1.CurrentRow.Cells[15].Value.ToString();
                record.Action = dtg1.CurrentRow.Cells[16].Value.ToString();
                record.PlanComplete = dtg1.CurrentRow.Cells[17].Value.ToString();
                record.ActualComplete = dtg1.CurrentRow.Cells[18].Value.ToString();
                record.Countermesure = dtg1.CurrentRow.Cells[19].Value.ToString();
                Global.dataRecords.Add(record);
                FormInput formInput = new FormInput();
                formInput.ShowDialog();
            }
            else
            {
                MessageBox.Show("Chưa chọn dữ liệu cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
