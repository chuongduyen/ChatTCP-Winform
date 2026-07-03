using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatLopHocClient
{
    public class ChatForm : Form
    {
        Socket client;
        Thread receiveThread;
        bool daKetNoi = false;

        TextBox txtIp;
        TextBox txtPort;
        TextBox txtTen;
        Button btnKetNoi;
        RichTextBox rtbChat;
        TextBox txtTinNhan;
        Button btnGui;
        Button btnDanhSach;
        Button btnThongBao;
        Button btnXemThongBao;
        Button btnTroGiup;
        Label lblHuongDan;

        public ChatForm()
        {
            TaoGiaoDien();
        }

        void TaoGiaoDien()
        {
            // Thiet lap form chinh
            Text = "Chat Lớp Học TCP - Client";
            Width = 930;
            Height = 620;
            MinimumSize = new Size(930, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
            AutoScaleMode = AutoScaleMode.None; // tranh bi lech giao dien khi may de scaling lon

            Label lblIp = new Label();
            lblIp.Text = "IP Server:";
            lblIp.Left = 15;
            lblIp.Top = 20;
            lblIp.Width = 85;
            Controls.Add(lblIp);

            txtIp = new TextBox();
            txtIp.Text = "127.0.0.1";
            txtIp.Left = 105;
            txtIp.Top = 17;
            txtIp.Width = 120;
            Controls.Add(txtIp);

            Label lblPort = new Label();
            lblPort.Text = "Cổng:";
            lblPort.Left = 245;
            lblPort.Top = 20;
            lblPort.Width = 55;
            Controls.Add(lblPort);

            txtPort = new TextBox();
            txtPort.Text = "12345";
            txtPort.Left = 305;
            txtPort.Top = 17;
            txtPort.Width = 80;
            Controls.Add(txtPort);

            Label lblTen = new Label();
            lblTen.Text = "Tên:";
            lblTen.Left = 405;
            lblTen.Top = 20;
            lblTen.Width = 45;
            Controls.Add(lblTen);

            txtTen = new TextBox();
            txtTen.Text = "Duy";
            txtTen.Left = 455;
            txtTen.Top = 17;
            txtTen.Width = 140;
            Controls.Add(txtTen);

            btnKetNoi = new Button();
            btnKetNoi.Text = "Kết nối";
            btnKetNoi.Left = 615;
            btnKetNoi.Top = 15;
            btnKetNoi.Width = 110;
            btnKetNoi.Height = 32;
            btnKetNoi.Click += BtnKetNoi_Click;
            Controls.Add(btnKetNoi);

            // Khung hien thi noi dung chat
            rtbChat = new RichTextBox();
            rtbChat.Left = 15;
            rtbChat.Top = 60;
            rtbChat.Width = 635;
            rtbChat.Height = 440;
            rtbChat.ReadOnly = true;
            rtbChat.BackColor = Color.White;
            rtbChat.ScrollBars = RichTextBoxScrollBars.Vertical;
            Controls.Add(rtbChat);

            // O nhap tin nhan
            txtTinNhan = new TextBox();
            txtTinNhan.Left = 15;
            txtTinNhan.Top = 515;
            txtTinNhan.Width = 525;
            txtTinNhan.Height = 30;
            txtTinNhan.KeyDown += TxtTinNhan_KeyDown;
            Controls.Add(txtTinNhan);

            btnGui = new Button();
            btnGui.Text = "Gửi";
            btnGui.Left = 555;
            btnGui.Top = 512;
            btnGui.Width = 95;
            btnGui.Height = 34;
            btnGui.Click += BtnGui_Click;
            Controls.Add(btnGui);

            // Khu vuc cac nut lenh nhanh
            GroupBox groupLenh = new GroupBox();
            groupLenh.Text = "Chức năng";
            groupLenh.Left = 675;
            groupLenh.Top = 60;
            groupLenh.Width = 225;
            groupLenh.Height = 225;
            Controls.Add(groupLenh);

            btnDanhSach = new Button();
            btnDanhSach.Text = "Danh sách online";
            btnDanhSach.Left = 18;
            btnDanhSach.Top = 32;
            btnDanhSach.Width = 185;
            btnDanhSach.Height = 32;
            btnDanhSach.Click += (s, e) => GuiLenhNhanh("/ds");
            groupLenh.Controls.Add(btnDanhSach);

            btnThongBao = new Button();
            btnThongBao.Text = "Soạn thông báo";
            btnThongBao.Left = 18;
            btnThongBao.Top = 75;
            btnThongBao.Width = 185;
            btnThongBao.Height = 32;
            btnThongBao.Click += (s, e) => ChenVaoOChat("/thongbao ");
            groupLenh.Controls.Add(btnThongBao);

            btnXemThongBao = new Button();
            btnXemThongBao.Text = "Xem thông báo";
            btnXemThongBao.Left = 18;
            btnXemThongBao.Top = 118;
            btnXemThongBao.Width = 185;
            btnXemThongBao.Height = 32;
            btnXemThongBao.Click += (s, e) => GuiLenhNhanh("/xemthongbao");
            groupLenh.Controls.Add(btnXemThongBao);

            btnTroGiup = new Button();
            btnTroGiup.Text = "Trợ giúp";
            btnTroGiup.Left = 18;
            btnTroGiup.Top = 161;
            btnTroGiup.Width = 185;
            btnTroGiup.Height = 32;
            btnTroGiup.Click += (s, e) => GuiLenhNhanh("/trogiup");
            groupLenh.Controls.Add(btnTroGiup);

            // Huong dan lenh cho nguoi dung
            lblHuongDan = new Label();
            lblHuongDan.Left = 675;
            lblHuongDan.Top = 305;
            lblHuongDan.Width = 235;
            lblHuongDan.Height = 210;
            lblHuongDan.Text = "Lệnh có thể dùng:\n" +
                              "/ds\n" +
                              "/rieng tên nội_dung\n" +
                              "/thongbao nội_dung\n" +
                              "/xemthongbao\n" +
                              "/trogiup";
            Controls.Add(lblHuongDan);
        }

        void BtnKetNoi_Click(object sender, EventArgs e)
        {
            if (!daKetNoi)
                KetNoiServer();
            else
                NgatKetNoi();
        }

        void KetNoiServer()
        {
            try
            {
                string ip = txtIp.Text.Trim();
                int port = int.Parse(txtPort.Text.Trim());
                string ten = txtTen.Text.Trim();

                if (ten == "")
                {
                    MessageBox.Show("Bạn chưa nhập tên.");
                    return;
                }

                // Tao socket va ket noi den server
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

                // Gui ten dau tien cho server
                client.Send(Encoding.UTF8.GetBytes(ten));

                daKetNoi = true;
                btnKetNoi.Text = "Ngắt";
                txtTen.Enabled = false;
                txtIp.Enabled = false;
                txtPort.Enabled = false;

                ThemDongChat("[Client] Đã kết nối đến server.");

                // Thread nhan tin tu server
                receiveThread = new Thread(NhanTinNhan);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không kết nối được server: " + ex.Message);
            }
        }

        void NhanTinNhan()
        {
            byte[] data = new byte[4096];

            try
            {
                while (daKetNoi)
                {
                    int size = client.Receive(data);
                    if (size == 0) break;

                    string msg = Encoding.UTF8.GetString(data, 0, size);
                    ThemDongChat(msg);
                }
            }
            catch
            {
                ThemDongChat("[Client] Đã mất kết nối với server.");
            }
        }

        void BtnGui_Click(object sender, EventArgs e)
        {
            GuiTinNhan();
        }

        void TxtTinNhan_KeyDown(object sender, KeyEventArgs e)
        {
            // Bam Enter de gui nhanh tin nhan
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                GuiTinNhan();
            }
        }

        void GuiTinNhan()
        {
            if (!daKetNoi)
            {
                MessageBox.Show("Bạn chưa kết nối server.");
                return;
            }

            string msg = txtTinNhan.Text.Trim();
            if (msg == "") return;

            try
            {
                client.Send(Encoding.UTF8.GetBytes(msg));
                txtTinNhan.Clear();
                txtTinNhan.Focus();
            }
            catch
            {
                ThemDongChat("[Client] Không gửi được tin nhắn.");
            }
        }

        void GuiLenhNhanh(string lenh)
        {
            if (!daKetNoi)
            {
                MessageBox.Show("Bạn chưa kết nối server.");
                return;
            }

            try
            {
                client.Send(Encoding.UTF8.GetBytes(lenh));
            }
            catch
            {
                ThemDongChat("[Client] Không gửi được lệnh.");
            }
        }

        void ChenVaoOChat(string text)
        {
            txtTinNhan.Text = text;
            txtTinNhan.SelectionStart = txtTinNhan.Text.Length;
            txtTinNhan.Focus();
        }

        void ThemDongChat(string msg)
        {
            // Invoke de cap nhat giao dien tu thread nhan tin
            if (InvokeRequired)
            {
                Invoke(new Action<string>(ThemDongChat), msg);
                return;
            }

            rtbChat.AppendText(msg + Environment.NewLine);
            rtbChat.ScrollToCaret();
        }

        void NgatKetNoi()
        {
            try
            {
                if (client != null)
                {
                    if (daKetNoi)
                        client.Send(Encoding.UTF8.GetBytes("/thoat"));

                    client.Close();
                }
            }
            catch { }

            daKetNoi = false;
            btnKetNoi.Text = "Kết nối";
            txtTen.Enabled = true;
            txtIp.Enabled = true;
            txtPort.Enabled = true;
            ThemDongChat("[Client] Đã ngắt kết nối.");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            NgatKetNoi();
            base.OnFormClosing(e);
        }
    }
}
