# Chat Lớp Học TCP

Đây là project chat nhóm đơn giản bằng C# Socket TCP.
Ứng dụng mô phỏng một phòng chat lớp học để sinh viên trao đổi bài tập, gửi thông báo và nhắn riêng.

## Công nghệ dùng

- C#
- Windows Forms cho Client
- Console App cho Server
- Socket TCP
- Thread xử lý nhiều client
- UTF-8 để gửi tiếng Việt

## Chức năng

1. Đăng nhập bằng tên sinh viên
2. Chat chung trong lớp
3. Xem danh sách online
4. Nhắn riêng cho một bạn
5. Gửi thông báo/bài tập cho cả lớp
6. Xem lại thông báo đã lưu

## Cách chạy

1. Mở `ChatLopHocTCP.sln` bằng Visual Studio 2022.
2. Chuột phải project `Server` -> `Set as Startup Project` -> chạy trước.
3. Sau đó chạy project `Client` nhiều lần để giả lập nhiều sinh viên.
4. Mỗi client nhập tên khác nhau rồi bấm `Kết nối`.

## Các lệnh sử dụng

```txt
/ds
```
Xem danh sách thành viên online.

```txt
/rieng An Nội dung cần nhắn
```
Nhắn riêng cho An.

```txt
/thongbao Mai nộp bài tập TCP Socket
```
Gửi thông báo hoặc bài tập cho cả lớp.

```txt
/xemthongbao
```
Xem lại danh sách thông báo đã lưu.

```txt
/trogiup
```
Xem hướng dẫn lệnh.

```txt
/thoat
```
Thoát khỏi lớp học.

## Kịch bản demo

1. Mở server.
2. Mở client Duy và client An.
3. Duy gửi tin nhắn bình thường.
4. An dùng `/ds` để xem ai đang online.
5. Duy dùng `/rieng An Cậu làm phần client nhé`.
6. Duy dùng `/thongbao Mai nộp bài lập trình mạng`.
7. An dùng `/xemthongbao` để xem lại thông báo.

## Cách giải thích khi vấn đáp

### Project dùng để làm gì?

Project là ứng dụng Chat Lớp Học TCP. Nhiều sinh viên có thể kết nối vào server để chat chung, nhắn riêng, xem danh sách online và gửi thông báo bài tập.

### Vì sao dùng TCP?

Vì TCP là giao thức hướng kết nối, dữ liệu gửi đi đáng tin cậy và đúng thứ tự. Ứng dụng chat cần nhận tin nhắn đầy đủ nên TCP phù hợp.

### Server xử lý nhiều client như thế nào?

Server dùng `Socket.Accept()` để nhận kết nối. Mỗi client sau khi kết nối sẽ được xử lý bằng một `Thread` riêng, nên nhiều client có thể chat cùng lúc.

### Lệnh nhắn riêng hoạt động như thế nào?

Khi client gửi `/rieng tên nội_dung`, server tách chuỗi để lấy tên người nhận. Sau đó server tìm client có tên đó trong danh sách và chỉ gửi tin nhắn đến người đó.

### Thông báo lớp học lưu ở đâu?

Thông báo được lưu tạm trong `List<string>` ở server. Khi client dùng `/xemthongbao`, server duyệt danh sách đó và gửi lại cho client.

## Phân công nhóm gợi ý

- Người 1: làm Server, xử lý socket, danh sách client, lệnh `/ds`, `/rieng`, `/thongbao`, `/xemthongbao`.
- Người 2: làm Client, giao diện WinForms, nút kết nối, gửi tin, nhận tin và hiển thị hướng dẫn lệnh.
