using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace CountermeasureManagement
{
    public partial class FormInput : Form

    {
        private static readonly object lockObj = new object();
        string connStr;
        MySQLHelper db;
        string _NO_;
        private string selectedImagePath = "";
        private string IP_ADRESSS = "";
        private string PORT = "";
        string IMAGE_URL = "";
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
            foreach (var line in System.IO.File.ReadAllLines(path))
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
                    System.IO.File.AppendAllText(logPath, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void LoadIpAddress()
        {
            try
            {
                var config = LoadConfig("Confighost.ini");
                IP_ADRESSS = config["IP"];
                PORT = config["PORT"];
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi LoadIpAddress: " + ex.Message);
            }
        }
        private async void FormInput_Load(object sender, EventArgs e)
        {
            LoadIpAddress();
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
                    await LoadDataAndShowImage(data.Image);
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
        private  void btnExit_Click(object sender, EventArgs e)
        {
            
             this.Close();
        }
        private async Task LoadDataAndShowImage(string imgUrl)
        {
            string imageUrlFromDb = imgUrl; // Ví dụ
            // 2. Hiển thị ảnh lên PictureBox
            try
            {
                if (!string.IsNullOrEmpty(imageUrlFromDb))
                {
                    picHinhAnh.Load(imageUrlFromDb);
                }
                else
                {
                    // Nếu không có ảnh, có thể hiển thị ảnh mặc định
                    picHinhAnh.Image = null; // Hoặc pictureBoxDisplay.Image = Properties.Resources.NoImage;
                }
            }
            catch (Exception ex)
            {
                // Xử lý trường hợp URL bị lỗi hoặc không truy cập được
                MessageBox.Show("Không thể tải ảnh: " + ex.Message);
                picHinhAnh.Image = null; // Hiển thị ảnh mặc định khi lỗi
            }
            await Task.Delay(10); // Giữ cho phương thức này là async
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
                        await SaveAndReturnUrlImageInServer();
                        await InsertData();
                    }
                }
                else
                {
                    if (_NO_ == "")
                    {
                        MessageBox.Show("Chưa có dữ liệu để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        DialogResult dlt = MessageBox.Show("Xác nhận sửa dữ liệu", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dlt == DialogResult.Yes)
                        {
                            await SaveAndReturnUrlImageInServer();
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
            else if (picHinhAnh.Image==null)
            {
                MessageBox.Show("Chưa chọn ảnh lỗi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    $",'{IMAGE_URL}','{richNoiDungLoi.Text}','{rd11}','{rd22}','{cbMucDoQuanTrong.Text}','{numQty.Value}',''," +
                    $"'{cbPhuongAnXuLy.Text}','{richActionTamThoi.Text}','{dtime2.Text}','{Global.Name}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')";
                await db.ExecuteNonQueryAsync(query);
                MessageBox.Show("Thêm dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            IMAGE_URL = "";
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
                    $"`pic_qc`='{cbPicPqc.Text.Trim()}',`image`='{IMAGE_URL}',`content_error`='{richNoiDungLoi.Text.Trim()}',`old_error`='{rd11}',`new_error`='{rd22}'," +
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
            IMAGE_URL = "";
        }
        private async Task SaveAndReturnUrlImageInServer()
        {
            try
            {
                // Nếu người dùng đã chọn một file ảnh
                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    // Upload ảnh lên server và nhận lại URL
                    using (WebClient client = new WebClient())
                    {
                        // URL của file PHP trên server XAMPP
                        string phpUploadUrl = $"http://{IP_ADRESSS}:{PORT}/quanlyloi/upload.php";

                        // Upload file và nhận về chuỗi response (chính là URL của ảnh)
                        byte[] response = client.UploadFile(phpUploadUrl, selectedImagePath);
                        IMAGE_URL = System.Text.Encoding.UTF8.GetString(response);

                        // Kiểm tra xem URL trả về có hợp lệ không
                        if (!IMAGE_URL.StartsWith("http"))
                        {
                            WriteLog("Lỗi upload ảnh: " + IMAGE_URL);
                            return; // Dừng lại nếu có lỗi
                        }
                    }
                }
                await Task.Delay(10);
                selectedImagePath = ""; // Reset đường dẫn sau khi lưu
            }
            catch (Exception ex)
            {
                WriteLog("Đã xảy ra lỗi try cacth SaveAndReturnUrlImageInServer: " + ex.Message);
            }
        }
        private void picHinhAnh_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = openFileDialog.FileName;
                // Hiển thị ảnh đã chọn lên một PictureBox để xem trước
                picHinhAnh.Image = Image.FromFile(selectedImagePath);
            }
        }
    }
}
