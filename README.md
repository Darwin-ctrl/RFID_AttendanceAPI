\# RFID Attendance API 🎓



ASP.NET Core Web API para sa RFID-based Attendance System.



\## 🚀 Tech Stack

\- ASP.NET Core 8

\- Entity Framework Core

\- PostgreSQL (sa Render)

\- Swagger/OpenAPI



\## 📋 API Endpoints



\### Attendance

\- `GET /api/attendance` - Get all attendance records

\- `POST /api/attendance` - Record new attendance



\### RFID

\- `POST /api/rfid/scan` - Process RFID scan

\- `GET /api/rfid/students` - Get all students



\## 🏃 How to Run Locally



```bash

dotnet restore

dotnet run

