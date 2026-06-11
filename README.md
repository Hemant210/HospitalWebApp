# 🏥 Hospital Management System

A modern Hospital Management System built with ASP.NET Core and SQL Server to streamline hospital operations, patient management, appointments, billing, and administrative workflows.

## 📌 Overview

The Hospital Management System is designed to simplify healthcare administration by providing a centralized platform for managing patients, doctors, appointments, medical records, billing, and hospital staff operations.

The application focuses on improving efficiency, reducing manual work, and ensuring secure management of healthcare data.

---

## ✨ Features

### 👨‍⚕️ Patient Management

* Add, update, and delete patient records
* Maintain patient profiles and medical history
* Search and filter patient information

### 🩺 Doctor Management

* Manage doctor profiles and specializations
* Assign doctors to patients
* View doctor schedules

### 📅 Appointment Management

* Book appointments
* Manage appointment schedules
* Track appointment status

### 💳 Billing System

* Generate patient bills
* Manage payments and invoices
* Track billing history

### 🔐 Authentication & Authorization

* Secure login system
* Role-based access control
* Protected administrative operations

### 📊 Dashboard

* Quick overview of hospital activities
* Statistics and operational insights

---

## 🛠️ Tech Stack

### Backend

* ASP.NET Core
* Entity Framework Core
* C#

### Database

* SQL Server

### Frontend

* Razor Views
* HTML5
* CSS3
* Bootstrap
* JavaScript

### Development Tools

* Visual Studio 2022
* Git & GitHub

---

## 📂 Project Structure

```text
HospitalWebApp/
│
├── HospitalManagement/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── Data/
│   ├── Services/
│   ├── wwwroot/
│   └── Program.cs
│
└── HospitalManagement.sln
```

---

## 🚀 Getting Started

### Prerequisites

* Visual Studio 2022
* .NET SDK
* SQL Server
* Git

### Clone Repository

```bash
git clone https://github.com/Hemant210/HospitalWebApp.git
cd HospitalWebApp
```

### Configure Database

Update the connection string inside:

```json
appsettings.json
```

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=HospitalDB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### Apply Database Migrations

Open Package Manager Console:

```powershell
Update-Database
```

### Run the Project

```bash
dotnet run
```

Or simply press:

```text
F5
```

in Visual Studio.

---

## 📸 Screenshots

Add screenshots here after deployment.

### Login Page

![Login](screenshots/login.png)

### Dashboard

![Dashboard](screenshots/dashboard.png)

### Patient Management

![Patient Management](screenshots/patient-management.png)

### Billing System

![Billing System](screenshots/billing-system.png)

---

## 🔒 Security Features

* Authentication and Authorization
* Input Validation
* SQL Injection Protection via Entity Framework
* Secure Database Access
* Role-Based Permissions

---

## 🌟 Future Enhancements

* Email Notifications
* Online Appointment Booking
* Telemedicine Integration
* Medical Report Uploads
* Prescription Management
* Analytics Dashboard
* Cloud Deployment

---

## 🤝 Contributing

Contributions are welcome.

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to your branch
5. Open a Pull Request

---
