# UDM_17 – Network Caro Game

## 1. Project Overview

**UDM_17 – Lập trình game cờ Caro qua mạng** là dự án phát triển một ứng dụng **game cờ Caro (Gomoku)** cho phép hai người chơi thi đấu với nhau thông qua **mô hình Client–Server sử dụng giao thức TCP**.

Ứng dụng được xây dựng với **giao diện đồ họa (GUI)** chạy trên hệ điều hành **Microsoft Windows**, cho phép người chơi kết nối tới server, gửi lời thách đấu, thực hiện nước đi và theo dõi thời gian suy nghĩ của từng lượt chơi.

Server chịu trách nhiệm quản lý các kết nối, điều phối trận đấu, kiểm tra tính hợp lệ của nước đi và lưu lại lịch sử các trận đấu đã diễn ra.

Dự án được thực hiện trong khuôn khổ học phần **Lập trình ứng dụng mạng (Network Application Programming)**.

---

## 2. Project Information

**Project Code:**
UDM_17

**Project Title:**
Lập trình game cờ Caro

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

Hệ thống bao gồm các chức năng chính sau:

* Client có thể **kết nối đến Server** thông qua mạng TCP.
* Người chơi có thể **gửi lời thách đấu** đến người chơi khác.
* Server **ghép cặp hai client** để bắt đầu trận đấu.
* Hai người chơi có thể **thi đấu cờ Caro trên cùng một bàn cờ** thông qua mạng.
* Hệ thống **đếm thời gian suy nghĩ cho mỗi lượt đi**.
* Server **kiểm tra tính hợp lệ của nước đi và xác định người thắng**.
* Server **lưu lại lịch sử trận đấu** để phục vụ thống kê và theo dõi.

---

## 5. System Architecture

Hệ thống được thiết kế theo mô hình:

**Client – Server Architecture**

```
Client (Player A)  <--TCP-->  Server  <--TCP-->  Client (Player B)
```

### Server Responsibilities

* Quản lý kết nối client
* Ghép cặp trận đấu
* Xử lý logic game
* Kiểm tra nước đi
* Quản lý thời gian lượt chơi
* Lưu lịch sử trận đấu

### Client Responsibilities

* Hiển thị giao diện người chơi (GUI)
* Gửi nước đi đến server
* Nhận và hiển thị trạng thái trận đấu
* Hiển thị thời gian suy nghĩ

---

## 6. Graphical User Interface (GUI)

Ứng dụng sử dụng **giao diện đồ họa (GUI)** để người dùng tương tác với hệ thống.

Các màn hình chính dự kiến gồm:

* **Login / Connect Screen** – Kết nối tới server
* **Lobby Screen** – Danh sách người chơi online
* **Game Board Screen** – Bàn cờ Caro
* **Match History Screen** – Lịch sử trận đấu

---

## 7. Technologies (Planned)

Các công nghệ dự kiến sử dụng:

* Programming Language: C# / Java / Python (tùy lựa chọn nhóm)
* Network Protocol: TCP Socket
* GUI Framework: Windows Forms / Java Swing / PyQt
* Data Storage: File hoặc SQLite (lưu lịch sử trận đấu)

---

## 8. Project Objectives

Mục tiêu của dự án:

* Áp dụng kiến thức **socket programming**
* Thiết kế **ứng dụng mạng theo mô hình Client–Server**
* Xây dựng **giao diện GUI hoàn chỉnh**
* Triển khai **logic game Caro trên môi trường mạng**
* Thực hiện **kiểm thử hiệu năng và kiểm thử chịu tải**

---

## 9. Status

Dự án hiện đang trong giai đoạn:

```
Planning & System Design
```

Các bước tiếp theo:

1. Thiết kế kiến trúc hệ thống
2. Xây dựng Server
3. Phát triển Client GUI
4. Tích hợp hệ thống
5. Kiểm thử hiệu năng và stress test
6. Hoàn thiện báo cáo

---

## 10. Course

**Course:** Network Application Programming
**Project Type:** Group Project
**Project Code:** UDM_17

---

© Net_Group 02

