# UDM_17 – Network Caro Game

---

## 1. Project Overview

**UDM_17 – Network Caro Game** là dự án xây dựng ứng dụng **game cờ Caro (Gomoku) chơi qua mạng** sử dụng mô hình **Client–Server** với giao thức **TCP Socket**.

Hệ thống cho phép nhiều người chơi kết nối đến server, thực hiện thi đấu 1vs1 theo thời gian thực. Toàn bộ logic game được xử lý phía server nhằm đảm bảo tính đồng bộ và chính xác.

Ứng dụng được phát triển bằng **C# (.NET) với Windows Forms**, cung cấp giao diện trực quan giúp người chơi dễ dàng thao tác, theo dõi trận đấu và tương tác với hệ thống.

Dự án được thực hiện trong khuôn khổ học phần **Network Programming**.

---

## 2. Project Information

**Project Code:**
UDM_17

**Project Title:**
Network Caro Game

**Group Code:**
Net_Group 02

---

## 3. Team Members

| No | Full Name         |
| -- | ----------------- |
| 1  | Hồ Ngọc Anh Thư   |
| 2  | Hoàng Thị Anh Thư |
| 3  | Lê Ngọc Châu      |
| 4  | Nguyễn Phúc Cảnh  |
| 5  | Huỳnh Thanh Kiệt  |
| 6  | Trần Minh Triết   |

---

## 4. Main Features

Hệ thống đã triển khai các chức năng chính sau:

### Kết nối & giao tiếp

* Client kết nối đến Server thông qua **TCP Socket**
* Server quản lý nhiều client đồng thời

### Gameplay
* Người chơi đăng nhập bằng username
* Người chơi có thể gửi lời thách đấu đến người chơi khác
* Chơi cờ Caro 1vs1 theo lượt (turn-based)
* Đồng bộ trạng thái bàn cờ theo thời gian thực và có đếm ngược thời gian mỗi lượt
* Hiển thị quân cờ X/O trực quan trên giao diện
* Kiểm tra thắng/thua theo luật Caro
* Lưu lịch sử trận đấu

### Đồng bộ dữ liệu

* Client gửi nước đi → Server xử lý → broadcast đến đối thủ
* Server đảm bảo:

  * Đúng lượt chơi
  * Nước đi hợp lệ 
  * Cập nhật trạng thái game chính xác

### Giao diện người dùng

* Giao diện Windows Forms trực quan
* Tương tác bằng click chuột trên bàn cờ và các nút chức năng
* Hiển thị trạng thái trận đấu

---

## 5. System Architecture

Hệ thống được xây dựng theo mô hình:

**Client – Server Architecture**

```
Client A  <--TCP-->  Server  <--TCP-->  Client B
```

---

### 🔹 Server Responsibilities

* Lắng nghe và quản lý kết nối từ nhiều client
* Điều phối trận đấu giữa các người chơi
* Xử lý logic game (turn, kiểm tra thắng/thua)
* Kiểm tra tính hợp lệ của nước đi
* Gửi dữ liệu đồng bộ đến các client

---

### 🔹 Client Responsibilities

* Hiển thị giao diện (GUI)
* Gửi yêu cầu (nước đi) đến server
* Nhận và cập nhật trạng thái game
* Hiển thị bàn cờ và kết quả trận đấu

---

## 6. Graphical User Interface (GUI)

Ứng dụng sử dụng **Windows Forms (WinForms)** để xây dựng giao diện.

Các màn hình chính:

* **Login Form**

  * Nhập username và kết nối server

* **Lobby Form**

  * Hiển thị danh sách người chơi online
  * Gửi lời thách đấu đến người chơi khác

* **Game Form**

  * Hiển thị bàn cờ Caro
  * Thực hiện nước đi
  * Hiển thị thông tin
  * Hiển thị đếm ngược thời gian mỗi lượt
  * Hiển thị lượt chơi và kết quả

* **History Form** 

  * Hiển thị lịch sử trận đấu đã chơi

---

## 7. Technologies Used

Các công nghệ sử dụng trong project:

* **Programming Language:** C# (.NET Framework)
* **GUI Framework:** Windows Forms
* **Network:** TCP Socket (`TcpClient`, `TcpListener`)
* **Architecture:** Client–Server
* **Concurrency:** Thread / Async xử lý nhiều client

---

## 8. Project Structure (Overview)

```bash
UDM_17/
│
├── Client/        # Ứng dụng phía người chơi (GUI + kết nối server)
├── Server/        # Server trung tâm (xử lý logic game & kết nối)
└── Shared/        # Thành phần dùng chung (Model, DTO, Message,…)
```
---

## 9. Project Objectives

Mục tiêu đạt được của dự án:

* Áp dụng kiến thức **Socket Programming (TCP)**
* Xây dựng hệ thống **Client–Server hoàn chỉnh**
* Thiết kế và phát triển **GUI bằng WinForms**
* Triển khai logic game Caro trên môi trường mạng
* Hiểu và xử lý **đồng bộ dữ liệu real-time**

---

## 10. Current Status

```
Completed – Functional System
```

### Đã hoàn thành:

* Xây dựng server TCP
* Phát triển client GUI
* Kết nối client–server
* Đăng nhập và gửi lời thách đấu tới người chơi khác
* Triển khai gameplay Caro
* Đồng bộ nước đi giữa 2 người chơi
* Lưu lịch sử trận đấu

### Hướng phát triển thêm:

* Thêm hệ thống phòng (room)
* Chat giữa người chơi
* Tối ưu đa luồng / async

---

## 11. How to Run

### ▶️ Server

1. Build project Server
2. Run ServerSocket
3. Server bắt đầu lắng nghe kết nối

### ▶️ Client

1. Build project Client
2. Chạy ứng dụng
3. Nhập username → kết nối server
4. Bắt đầu chơi

---

## 12. Course Information

**Course:** Network Programming
**Project Type:** Group Project
**Project Code:** UDM_17

---

© Net_Group 02
