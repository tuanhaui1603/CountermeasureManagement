using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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

            Global.Name = "Võ Quang Tuấn";
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
                Global.dataRecords.Clear();
                string query = "SELECT data.`no`, data.date as 'Ngày', data.status_error as 'Tình trạng lỗi', data.part_name as 'PartName'," +
                    " data.area as 'Khu vực phát sinh', data.ncc_c1 as 'NCC Cấp1', data.ncc_c2 as 'NCC Cấp2', data.pic_qc as 'PIC QC'," +
                    " `image`, data.content_error as 'Nội dung lỗi', data.old_error as 'Cũ',data.new_error as 'Mới',data.rank as 'Rank'," +
                    " data.qty as 'Qty', data.qty_total as 'Qty Total', data.solution as 'Phương án xử lý lỗi', data.action as 'Action tạm thời'," +
                    "data.plan_complete as 'Plan ht đối sách',data_reason_solution.actual_date_completed_plan as 'Thực tế ht đối sách'," +
                    "data_reason_solution.reason as 'Nguyên nhân',data_reason_solution.solution as 'Đối sách' " +
                    "FROM data LEFT JOIN data_reason_solution ON data.no = data_reason_solution.no_id " +
                    $"where data.date between '{dtime1.Text}' and '{dtime2.Text}' and data.ncc_c1 like '%{tbNccSearch.Text.Trim()}%' " +
                    $"and data.rank like '%{cbRankSearch.Text.Trim()}%' order by data.date DESC";
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
            DataTable dt = dtg1.DataSource as DataTable;
            if (dt != null && dt.Rows.Count > 0)
            {
                ExportToExcel(dt);
            }
            else
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void ExportToExcel(DataTable dataTable)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            saveFileDialog.Title = "Chọn nơi lưu file Excel";
            saveFileDialog.FileName = $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var filePath = saveFileDialog.FileName;
                    var fileInfo = new FileInfo(filePath);

                    using (var package = new ExcelPackage(fileInfo))
                    {
                        var worksheet = package.Workbook.Worksheets.Add("DuLieu");

                        // Load dữ liệu từ DataTable vào, bắt đầu từ ô A1, có kèm header
                        worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                        // --- BẮT ĐẦU ĐỊNH DẠNG ---

                        // Lấy ra vùng dữ liệu đã được load vào
                        var dataRange = worksheet.Cells[worksheet.Dimension.Address];

                        // 1. ĐỊNH DẠNG NGÀY THÁNG (MM/dd/yyyy) CHO CỘT 1 VÀ 17
                        // Lưu ý: Cột trong EPPlus được tính bắt đầu từ 1
                        worksheet.Column(2).Style.Numberformat.Format = "MM/dd/yyyy";
                        worksheet.Column(18).Style.Numberformat.Format = "MM/dd/yyyy";

                        // 2. KẺ VIỀN (BORDER) CHO TẤT CẢ DỮ LIỆU
                        dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        // 3. TÔ MÀU NỀN XÁM CHO TIÊU ĐỀ
                        // Lấy ra vùng tiêu đề (dòng đầu tiên)
                        var headerRange = worksheet.Cells[1, 1, 1, dataTable.Columns.Count];
                        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        headerRange.Style.Font.Bold = true; // (Tùy chọn) In đậm chữ tiêu đề

                        // Tự động điều chỉnh độ rộng các cột
                        worksheet.Cells.AutoFitColumns();

                        // --- KẾT THÚC ĐỊNH DẠNG ---

                        package.Save();
                    }

                    // Hiển thị thông báo thành công
                    var result = MessageBox.Show("Xuất file Excel thành công!\nBạn có muốn mở file vừa tạo không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    // 4. MỞ FILE EXCEL SAU KHI XUẤT
                    if (result == DialogResult.Yes)
                    {
                        // Sử dụng Process.Start để mở file
                        // Cần thêm thuộc tính UseShellExecute = true cho .NET Core/5/6 trở lên
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.ToString(), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (_NO_ != "" && _NO_ != null)
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

        private void btnUpdateDoiSach_Click(object sender, EventArgs e)
        {
            if (_NO_ != "" && _NO_ != null)
            {
                Global.dataRecords = new List<DataRecord>();
                DataRecord record = new DataRecord();
                record.No = dtg1.CurrentRow.Cells[0].Value.ToString();
                Global.dataRecords.Add(record);
                FormUpdateSolution fom = new FormUpdateSolution();
                fom.ShowDialog();
            }
            else
            {
                MessageBox.Show("Chưa chọn dữ liệu cần update!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
