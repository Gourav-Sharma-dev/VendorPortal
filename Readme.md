# 🏢 Vendor Portal

A full-stack web application built with **ASP.NET Core Web API** (Backend) and **Angular** (Frontend), using **Entity Framework Core** for database management.

---

## 🛠️ Tech Stack

| Layer     | Technology                          |
|-----------|-------------------------------------|
| Frontend  | Angular                             |
| Backend   | ASP.NET Core Web API (.NET)         |
| ORM       | Entity Framework Core               |
| Database  | SQL Server                          |
| Auth      | JWT (JSON Web Tokens) + Role-Based  |

---

## 📁 Project Structure

```
VendorPortal/
├── Client/          # Angular Frontend
└── Server/          # ASP.NET Core Web API Backend
```

---

## ⚙️ Prerequisites

Make sure you have the following installed before running the project:

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or later)
- [Node.js](https://nodejs.org/) (version 18 or later)
- [Angular CLI](https://angular.io/cli) → `npm install -g @angular/cli`
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (or SQL Server Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

---

## 🚀 Getting Started

### 1️⃣ Backend Setup (ASP.NET Core Web API)

#### Step 1 — Configure the Database Connection

Open `Server/appsettings.json` and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=VendorPortalDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> Replace `YOUR_SERVER_NAME` with your SQL Server instance name (e.g., `localhost` or `.\SQLEXPRESS`).

#### Step 2 — Apply Database Migrations

Open a terminal in the `Server/` folder and run:

```bash
cd Server
dotnet ef database update
```

This will create the database and all required tables automatically.

#### Step 3 — Run the Backend

```bash
dotnet run
```

The API will start at:
- `https://localhost:7063` (or check your terminal for the exact port)

---

### 2️⃣ Frontend Setup (Angular)

#### Step 1 — Install Dependencies

Open a terminal in the `Client/` folder and run:

```bash
cd Client
npm install
```

#### Step 2 — Run the Frontend

```bash
ng serve
```

The Angular app will be available at:
- `http://localhost:4200`

> Make sure the backend is running before using the frontend.

---

## 🔐 Authentication & Login Process

### How to Register & Login

1. Open the app at `http://localhost:4200`
2. Click **Register** to create a new account
3. Fill in your details and submit
4. After registration, you will be assigned a **default role**
5. Use your credentials to **Login**
6. On successful login, a **JWT token** is issued and stored for session management

---

## 👥 User Roles & Access

| Role      | Access & Permissions                                                                 |
|-----------|--------------------------------------------------------------------------------------|
| **Admin** | Full access — manage users, change roles, manage vendors, view all data              |
| **Vendor**| Can manage their own profile, submit and view their vendor-related data              |
| **Procurement**  | Review and approve vendor information                                         |
| **Finance**  | Verify financial and tax details                                                  |
----------------------------------------------------------------------------------------------------

## 🔑 Admin Role Setup (First Time Only — Manual Step Required)
> After Register on Portal user have only vendot access
> ⚠️ **Important:** The very first Admin must be set up **manually** in the database. insert the UserRoles table with Admin role

### Steps to Create the First Admin:

#### Step 1 — Register a new user through the portal

Go to `http://localhost:4200` and register normally.

#### Step 2 — Find the User ID

Open **SQL Server Management Studio (SSMS)** or any DB client and run:

```sql
SELECT *
FROM Users 
WHERE Email = 'your-email@example.com';
```

Copy the `Id` value of the user.

#### Step 3 — Find the Admin Role ID

```sql
SELECT * 
FROM Roles;
```

Copy the `Id` value of the Admin role.

#### Step 4 — Assign the Admin Role Manually

Insert a record into the `UserRoles` table:

```sql
INSERT INTO UserRoles (UserId, RoleId)
VALUES ('YOUR_USER_ID', 'YOUR_ROLE_ID');
```

> Replace `YOUR_USER_ID` and `YOUR_ROLE_ID` with the values you copied above.

#### Step 5 — Log in as Admin

Now log in with that user's credentials on the portal.  
The system will recognize the **Admin role** and grant full access.

---

## 👮 Admin — Managing User Roles (After First Login)

Once logged in as Admin, you can **change roles of any registered user** directly through the portal:

1. Navigate to the **Admin Panel** or **User Management** section
2. Search or browse the list of registered users
3. Select a user and choose a new role from the dropdown
4. Save the changes — the role is updated immediately

> ✅ No manual database changes needed after the first Admin is set up.

---

## 📌 Quick Reference — Running the Project

| Step | Command                  | Directory  |
|------|--------------------------|------------|
| 1    | `dotnet ef database update` | `Server/` |
| 2    | `dotnet run`             | `Server/`  |
| 3    | `npm install`            | `Client/`  |
| 4    | `ng serve`               | `Client/`  |

---

## 🐛 Common Issues & Fixes

| Issue                            | Fix                                                                 |
|----------------------------------|---------------------------------------------------------------------|
| DB connection error              | Check `appsettings.json` connection string                          |
| Port already in use              | Change port in `launchSettings.json` or `angular.json`             |
| CORS error                       | Ensure backend has CORS configured for `http://localhost:4200`     |
| `ng` command not found           | Run `npm install -g @angular/cli`                                   |
| Migration not found              | Run `dotnet ef migrations add InitialCreate` then `database update` |

---

## 📞 Support

If you encounter any issues, please raise an issue in the repository or contact the project maintainer.
