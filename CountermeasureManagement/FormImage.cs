using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CountermeasureManagement
{
    public partial class FormImage : Form
    {
        public FormImage()
        {
            InitializeComponent();
        }

        private async void FormImage_Load(object sender, EventArgs e)
        {
            await LoadDataAndShowImage(Global.ImageUrl);
        }
        private async Task LoadDataAndShowImage(string imgUrl)
        {
            string imageUrlFromDb = imgUrl; // Ví dụ
            // 2. Hiển thị ảnh lên PictureBox
            try
            {
                if (!string.IsNullOrEmpty(imageUrlFromDb))
                {
                    picImage.Load(imageUrlFromDb);
                }
                else
                {
                    // Nếu không có ảnh, có thể hiển thị ảnh mặc định
                    picImage.Image = null; // Hoặc pictureBoxDisplay.Image = Properties.Resources.NoImage;
                }
            }
            catch (Exception ex)
            {
                // Xử lý trường hợp URL bị lỗi hoặc không truy cập được
                MessageBox.Show("Không thể tải ảnh: " + ex.Message);
                picImage.Image = null; // Hiển thị ảnh mặc định khi lỗi
            }
            await Task.Delay(10); // Giữ cho phương thức này là async
        }
    }
}
