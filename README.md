# Agile Project

## 🚀 Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) **version 8.0 or higher**
- [XAMPP](https://www.apachefriends.org/) (Apache + MySQL)
- [Git](https://git-scm.com/)

---

### Installation

#### 1. Clone the repository

```bash
git clone https://github.com/nguyenbaolab/agile_project
cd agile_project
```

#### 2. Start XAMPP

- Open **XAMPP Control Panel**
- Click **Start** on **Apache**
- Click **Start** on **MySQL**

#### 3. Build & run the application

Open a terminal in the project directory and run:

```bash
dotnet build
dotnet run
```

The application will start automatically. Open your browser and navigate to the URL shown in the terminal (usually `http://localhost:5000` or `https://localhost:5001`).

---

## 👤 Default Accounts

| Username | Password | Role |
|----------|----------|------|
| `admin` | `admin123` | Admin |
| `po` | `po123` | Product Owner |
| `dev` | `dev123` | Developer |

---

## 📊 Burndown Chart – Setup Guide

### Requirement
The `System.Windows.Forms.DataVisualization` library must be added to the project.

---

### 🔍 Check your project type first

Right-click the project → **Properties** → check the **Target framework**:
- `.NET Framework 4.x` → use **Method 1**
- `.NET 5 / 6 / 7 / 8` → use **Method 2**

---

### Method 1: Add Reference (for .NET Framework)

1. Right-click the project in **Solution Explorer**
2. Select **Add → Reference**
3. Go to the **Assemblies** tab
4. Find and check `System.Windows.Forms.DataVisualization`
5. Click **OK**

---

### Method 2: NuGet Package Manager (for .NET 5/6/7/8)

Open the **Package Manager Console** and run:

```bash
dotnet add package System.Windows.Forms.DataVisualization --prerelease
```

> ⚠️ The `--prerelease` flag is required because this package does not have a stable release yet.

---

### 🚀 After Setup

Once the library is added successfully, the **Burndown Chart** tab in `ReportsForm.cs` will work correctly.

---

## 🛠️ Troubleshooting

### ✅ Fix: Missing `System.Data.SqlClient` Assembly

If you see this error when opening the Reports tab:

```
System.IO.FileNotFoundException: Could not load file or assembly 'System.Data.SqlClient, Version=4.6.0.0'
```

Run the following command in the project directory:

```bash
dotnet add package System.Data.SqlClient
```

Then rebuild and run:

```bash
dotnet build
dotnet run
```