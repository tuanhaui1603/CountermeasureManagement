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
            Global.User = "";
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
        private void ResetSummary()
        {
            lbSumTongSoLoi.Text = lbSumMasV.Text = lbSumKhacHang.Text
            = lbSumDaHoanThanh.Text = lbSumChuaHoanThanh.Text = lbSumQuaHan.Text
            = lbSumRankA.Text = lbSumRankB.Text = lbSumRankC.Text
            = lbSumRankPhanNan.Text = lbSumRankTuChoi.Text = lbSumRankOther.Text
           = lbSum4mConNguoi.Text = lbSum4mPhuongPhap.Text = lbSum4mmayMoc.Text = lbSum4mVatLieu.Text = "0";
        }
        private void ResetSummary4MnguyenNhan()
        {
           lbSum4mConNguoi.Text = lbSum4mPhuongPhap.Text = lbSum4mmayMoc.Text = lbSum4mVatLieu.Text = "0";
        }
        private async Task LoadSummary()
        {
            ResetSummary();
            string query = "SELECT COUNT(*) AS 'Tổng số lỗi', SUM(CASE WHEN `area` IN ('ASSY', 'IQC', 'OQC') THEN 1 ELSE 0 END) AS 'Mas-V'," +
                "\r\n    SUM(CASE WHEN `area` = 'Khách hàng' THEN 1 ELSE 0 END) AS 'Khách hàng'," +
                "\r\n    SUM(CASE WHEN `status_error` = 'O' THEN 1 ELSE 0 END) AS 'Đã hoàn thành'," +
                "\r\n    SUM(CASE WHEN `status_error` = 'Y' THEN 1 ELSE 0 END) AS 'Chưa hoàn thành'," +
                "\r\n    SUM(CASE WHEN `status_error` = 'X' THEN 1 ELSE 0 END) AS 'Quá hạn'," +
                "\r\n    SUM(CASE WHEN `rank` = 'A' THEN 1 ELSE 0 END) AS 'RankA'," +
                "\r\n    SUM(CASE WHEN `rank` = 'B' THEN 1 ELSE 0 END) AS 'RankB'," +
                "\r\n    SUM(CASE WHEN `rank` = 'C' THEN 1 ELSE 0 END) AS 'RankC'," +
                "\r\n    SUM(CASE WHEN `rank` = 'Phàn nàn' THEN 1 ELSE 0 END) AS 'Phàn nàn'," +
                "\r\n    SUM(CASE WHEN `rank` = 'Từ chối' THEN 1 ELSE 0 END) AS 'Từ chối'," +
                "\r\n    SUM(CASE WHEN `rank` = 'Other' THEN 1 ELSE 0 END) AS 'Other'" +
                $"\r\nFROM `data` WHERE `date` BETWEEN '{dtime1.Text}' and '{dtime2.Text}'";
            DataTable dt = await db.GetDataTableAsync(query);
            if (dt != null && dt.Rows.Count > 0)
            {
                lbSumTongSoLoi.Text = (dt.Rows[0][0].ToString() != "") ? dt.Rows[0][0].ToString() : "0";
                lbSumMasV.Text = (dt.Rows[0][1].ToString() != "") ? dt.Rows[0][1].ToString() : "0";
                lbSumKhacHang.Text = (dt.Rows[0][2].ToString() != "") ? dt.Rows[0][2].ToString() : "0";
                lbSumDaHoanThanh.Text = (dt.Rows[0][3].ToString() != "") ? dt.Rows[0][3].ToString() : "0";
                lbSumChuaHoanThanh.Text = (dt.Rows[0][4].ToString() != "") ? dt.Rows[0][4].ToString() : "0";
                lbSumQuaHan.Text = (dt.Rows[0][5].ToString() != "") ? dt.Rows[0][5].ToString() : "0";
                lbSumRankA.Text = (dt.Rows[0][6].ToString() != "") ? dt.Rows[0][6].ToString() : "0";
                lbSumRankB.Text = (dt.Rows[0][7].ToString() != "") ? dt.Rows[0][7].ToString() : "0";
                lbSumRankC.Text = (dt.Rows[0][8].ToString() != "") ? dt.Rows[0][8].ToString() : "0";
                lbSumRankPhanNan.Text = (dt.Rows[0][9].ToString() != "") ? dt.Rows[0][9].ToString() : "0";
                lbSumRankTuChoi.Text = (dt.Rows[0][10].ToString() != "") ? dt.Rows[0][10].ToString() : "0";
                lbSumRankOther.Text = (dt.Rows[0][11].ToString() != "") ? dt.Rows[0][11].ToString() : "0";
            }
        }
        private async Task LoadSummary4MNguyenNhan()
        {
            ResetSummary4MnguyenNhan();
            string query = "SELECT \r\nSUM(CASE WHEN `con_nguoi` = '1' THEN 1 ELSE 0 END) AS 'Con người' ," +
                "\r\nSUM(CASE WHEN `phuong_phap` = '1' THEN 1 ELSE 0 END) AS 'Phương pháp' ," +
                "\r\nSUM(CASE WHEN `may_moc` = '1' THEN 1 ELSE 0 END) AS 'Máy móc' ," +
                "\r\nSUM(CASE WHEN `vat_lieu` = '1' THEN 1 ELSE 0 END) AS 'Vật liệu'" +
                " \r\nFROM `data` LEFT JOIN reason ON data.no = reason.no_id" +
                $" \r\nWHERE `date` BETWEEN '{dtime1.Text}' and '{dtime2.Text}'";
            DataTable dt = await db.GetDataTableAsync(query);
            if (dt != null && dt.Rows.Count > 0)
            {
                lbSum4mConNguoi.Text = (dt.Rows[0][0].ToString() != "") ? dt.Rows[0][0].ToString() : "0";
                lbSum4mPhuongPhap.Text = (dt.Rows[0][1].ToString() != "") ? dt.Rows[0][1].ToString() : "0";
                lbSum4mmayMoc.Text = (dt.Rows[0][2].ToString() != "") ? dt.Rows[0][2].ToString() : "0";
                lbSum4mVatLieu.Text = (dt.Rows[0][3].ToString() != "") ? dt.Rows[0][3].ToString() : "0";
            }
        }
        private async void btnLamMoi_Click(object sender, EventArgs e)
        {
            btnLamMoi.Enabled = false;
            btnLamMoi.Text = "Đang tải...";
            await LoadDataAsync();
            await LoadSummary();
            await LoadSummary4MNguyenNhan();
            btnLamMoi.Text = "Làm mới";
            btnLamMoi.Enabled = true;
        }
        private async Task LoadDataAsync()
        {
            try
            {
                _NO_ = "";
                Global.dataRecords.Clear();
                string query = "SELECT data.`no`,  data.date as 'Ngày', data.status_error as 'Tình trạng lỗi',data.part_name as 'PartName', " +
                    "\r\n    data.area as 'Khu vực phát sinh', " +
                    "\r\n    data.ncc_c1 as 'NCC Cấp1'," +
                    "\r\n    data.ncc_c2 as 'NCC Cấp2', " +
                    "\r\n    data.pic_qc as 'PIC QC', " +
                    "\r\n    `image` as 'Hình ảnh', " +
                    "\r\n    data.content_error as 'Nội dung lỗi', " +
                    "\r\n    data.old_error as 'Cũ'," +
                    "\r\n    data.new_error as 'Mới'," +
                    "\r\n    data.rank as 'Rank', " +
                    "\r\n    data.qty as 'Qty', " +
                    "\r\n    data.qty_total as 'Qty Total', " +
                    "\r\n    data.solution as 'Phương án xử lý lỗi', " +
                    "\r\n    data.action as 'Action tạm thời'," +
                    "\r\n    data.plan_complete as 'Plan ht đối sách'," +
                    "\r\n    data_reason_solution.actual_date_completed_plan as 'Thực tế ht đối sách'," +
                    "\r\n    data_reason_solution.reason as 'Nguyên nhân'," +
                    "\r\n    data_reason_solution.solution as 'Đối sách'," +
                    "\r\n   reason.con_nguoi as '4M(nn):Con người'," +
                    "\r\n    reason.phuong_phap as '4M(nn):Phương pháp'," +
                    "\r\n    reason.may_moc as '4M(nn):Máy móc'," +
                    "\r\n    reason.vat_lieu as '4M(nn): Vật liệu'," +
                    "\r\n    method.con_nguoi as '4M(pp):Con người'," +
                    "\r\n    method.phuong_phap as '4M(pp):Phương pháp'," +
                    "\r\n    method.may_moc as '4M(pp):Máy móc'," +
                    "\r\n    method.vat_lieu as '4M(pp):Vật liệu'" +
                    "\r\n   FROM data LEFT JOIN data_reason_solution ON data.no = data_reason_solution.no_id LEFT JOIN" +
                    "  reason ON data.no = reason.no_id LEFT JOIN  method ON data.no = method.no_id " +
                    $"WHERE data.date BETWEEN '{dtime1.Text}' and '{dtime2.Text}'" +
                    $" and data.ncc_c1 like '%{tbNccSearch.Text.Trim()}%'" +
                    $" and data.rank like '%{cbRankSearch.Text.Trim()}%' ORDER BY data.date DESC;";
                dtg1.DataSource = await db.GetDataTableAsync(query);
                if (Global.CheckExecuteQueryMySql)
                {
                    dtg1.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9);
                    dtg1.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11);
                    dtg1.Columns[0].DefaultCellStyle.ForeColor = Color.Blue;
                    dtg1.Columns[8].DefaultCellStyle.ForeColor = Color.Blue;
                    dtg1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    dtg1.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    dtg1.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    dtg1.Columns[9].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    dtg1.Columns[16].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    dtg1.Columns[19].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                }
                else
                {
                    MessageBox.Show("Lỗi kết nối database: " + Global.MessageErrorExecuteQueryMySql, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WriteLog("Lỗi kết nối database: " + Global.MessageErrorExecuteQueryMySql);
                }

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
                        if (Global.CheckExecuteQueryMySql)
                        {
                            MessageBox.Show("Xóa dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadDataAsync();
                        }
                        else
                        {
                            MessageBox.Show("Lỗi xóa dữ liệu: " + Global.MessageErrorExecuteQueryMySql, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            WriteLog("Lỗi xóa dữ liệu: " + Global.MessageErrorExecuteQueryMySql);
                        }
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
                else if (e.ColumnIndex == 8)
                {
                    Global.ImageUrl = dtg1.Rows[e.RowIndex].Cells[8].Value.ToString();
                    FormImage formImage = new FormImage();
                    formImage.ShowDialog();
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
                if (dtg1.CurrentRow.Cells[19].Value.ToString() == "")
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
                    MessageBox.Show("Bạn đã update đối sách này rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Chưa chọn dữ liệu cần update!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
