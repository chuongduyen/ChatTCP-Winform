using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatLopHocServer
{
    class Program
    {
        static List<Socket> clients = new List<Socket>();
        static List<string> clientNames = new List<string>();
        static List<string> thongBaos = new List<string>();

        // Dung lock de tranh loi khi nhieu client vao/ra cung luc
        static object khoa = new object();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Server Chat Lớp Học TCP";

            Console.WriteLine("===== SERVER CHAT LỚP HỌC TCP =====");

            // Tao socket server
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Gan server vao cong 12345
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 12345);
            server.Bind(ep);

            // Cho phep toi da 10 client cho ket noi
            server.Listen(10);

            Console.WriteLine("Server đang chạy tại cổng 12345...");
            Console.WriteLine("Đang chờ sinh viên kết nối...");
            Console.WriteLine();

            // Chap nhan nhieu client
            while (true)
            {
                Socket client = server.Accept();

                // Moi client duoc xu ly bang mot thread rieng
                Thread t = new Thread(() => HandleClient(client));
                t.IsBackground = true;
                t.Start();
            }
        }

        static void HandleClient(Socket client)
        {
            byte[] data = new byte[4096];
            string username = "";

            try
            {
                // Nhan ten sinh vien dau tien tu client
                int size = client.Receive(data);
                username = Encoding.UTF8.GetString(data, 0, size).Trim();

                if (username == "") username = "Khach";
                username = TaoTenKhongTrung(username);

                lock (khoa)
                {
                    clients.Add(client);
                    clientNames.Add(username);
                }

                Console.WriteLine("[+] " + username + " đã vào lớp. Tổng: " + clients.Count + " người");

                GuiChoMotNguoi(client, "[Hệ thống] Chào " + username + ", bạn đã vào phòng chat lớp học.");
                GuiChoMotNguoi(client, HuongDanLenh());
                Broadcast("[Hệ thống] " + username + " đã tham gia lớp học.", client);

                // Nhan tin nhan lien tuc tu client
                while (true)
                {
                    size = client.Receive(data);
                    if (size == 0) break;

                    string msg = Encoding.UTF8.GetString(data, 0, size).Trim();
                    if (msg == "") continue;

                    XuLyTinNhan(client, username, msg);
                }
            }
            catch
            {
                // Client tat cua so dot ngot thi server khong bi dung
            }
            finally
            {
                // Xoa client khoi danh sach khi ngat ket noi
                lock (khoa)
                {
                    int i = clients.IndexOf(client);
                    if (i >= 0)
                    {
                        clients.RemoveAt(i);
                        clientNames.RemoveAt(i);
                    }
                }

                try { client.Close(); } catch { }

                if (username != "")
                {
                    Console.WriteLine("[-] " + username + " đã rời lớp. Còn lại: " + clients.Count + " người");
                    Broadcast("[Hệ thống] " + username + " đã rời lớp học.", null);
                }
            }
        }

        static void XuLyTinNhan(Socket client, string username, string msg)
        {
            // Lenh /ds: xem danh sach thanh vien online
            if (msg == "/ds")
            {
                GuiChoMotNguoi(client, LayDanhSachOnline());
                return;
            }

            // Lenh /rieng: nhan tin rieng cho 1 ban trong lop
            if (msg.StartsWith("/rieng "))
            {
                XuLyNhanRieng(client, username, msg);
                return;
            }

            // Lenh /thongbao: luu thong bao/bai tap cho lop
            if (msg.StartsWith("/thongbao "))
            {
                string noiDung = msg.Substring(10).Trim();
                if (noiDung == "")
                {
                    GuiChoMotNguoi(client, "[Hệ thống] Bạn chưa nhập nội dung thông báo.");
                    return;
                }

                string tb = DateTime.Now.ToString("HH:mm") + " - " + username + ": " + noiDung;
                lock (khoa)
                {
                    thongBaos.Add(tb);
                }

                Console.WriteLine("[Thông báo] " + tb);
                Broadcast("[Thông báo lớp học] " + tb, null);
                return;
            }

            // Lenh /xemthongbao: xem lai thong bao da luu
            if (msg == "/xemthongbao")
            {
                GuiChoMotNguoi(client, LayThongBao());
                return;
            }

            // Lenh /trogiup: xem huong dan lenh
            if (msg == "/trogiup")
            {
                GuiChoMotNguoi(client, HuongDanLenh());
                return;
            }

            // Lenh /thoat: ngat ket noi
            if (msg == "/thoat")
            {
                GuiChoMotNguoi(client, "[Hệ thống] Bạn đã thoát khỏi lớp học.");
                client.Close();
                return;
            }

            // Tin nhan binh thuong se gui cho ca lop
            string formatted = "[" + username + "]: " + msg;
            Console.WriteLine(formatted);
            Broadcast(formatted, null);
        }

        static void XuLyNhanRieng(Socket sender, string fromName, string msg)
        {
            // Dang lenh: /rieng TenBan Noi dung
            string content = msg.Substring(7).Trim();
            int spaceIndex = content.IndexOf(' ');

            if (spaceIndex < 0)
            {
                GuiChoMotNguoi(sender, "[Hệ thống] Cú pháp đúng: /rieng tên nội_dung");
                return;
            }

            string toName = content.Substring(0, spaceIndex).Trim();
            string privateMsg = content.Substring(spaceIndex + 1).Trim();

            Socket receiver = null;
            lock (khoa)
            {
                for (int i = 0; i < clientNames.Count; i++)
                {
                    if (clientNames[i].Equals(toName, StringComparison.OrdinalIgnoreCase))
                    {
                        receiver = clients[i];
                        break;
                    }
                }
            }

            if (receiver == null)
            {
                GuiChoMotNguoi(sender, "[Hệ thống] Không tìm thấy bạn có tên: " + toName);
                return;
            }

            GuiChoMotNguoi(receiver, "[Tin riêng từ " + fromName + "]: " + privateMsg);
            GuiChoMotNguoi(sender, "[Bạn gửi riêng cho " + toName + "]: " + privateMsg);
            Console.WriteLine("[Riêng] " + fromName + " -> " + toName + ": " + privateMsg);
        }

        static string LayDanhSachOnline()
        {
            string result = "[Danh sách online]\n";

            lock (khoa)
            {
                if (clientNames.Count == 0)
                {
                    return "[Danh sách online] Hiện chưa có ai online.";
                }

                for (int i = 0; i < clientNames.Count; i++)
                {
                    result += (i + 1) + ". " + clientNames[i] + "\n";
                }
            }

            return result.TrimEnd();
        }

        static string LayThongBao()
        {
            string result = "[Thông báo/bài tập đã lưu]\n";

            lock (khoa)
            {
                if (thongBaos.Count == 0)
                {
                    return "[Thông báo] Hiện chưa có thông báo hoặc bài tập nào.";
                }

                for (int i = 0; i < thongBaos.Count; i++)
                {
                    result += (i + 1) + ". " + thongBaos[i] + "\n";
                }
            }

            return result.TrimEnd();
        }

        static string HuongDanLenh()
        {
            return "[Hướng dẫn]\n" +
                   "/ds - xem danh sách online\n" +
                   "/rieng tên nội_dung - nhắn riêng\n" +
                   "/thongbao nội_dung - gửi thông báo/bài tập\n" +
                   "/xemthongbao - xem lại thông báo\n" +
                   "/thoat - thoát khỏi lớp học";
        }

        static string TaoTenKhongTrung(string name)
        {
            // Neu trung ten thi them so phia sau, vi du Duy2
            string newName = name;
            int count = 2;

            lock (khoa)
            {
                while (clientNames.Contains(newName))
                {
                    newName = name + count;
                    count++;
                }
            }

            return newName;
        }

        static void GuiChoMotNguoi(Socket client, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                client.Send(data);
            }
            catch { }
        }

        // Gui tin nhan cho tat ca client, co the bo qua nguoi gui
        static void Broadcast(string message, Socket except)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            lock (khoa)
            {
                foreach (Socket c in clients)
                {
                    if (c == except) continue;
                    try { c.Send(data); } catch { }
                }
            }
        }
    }
}
