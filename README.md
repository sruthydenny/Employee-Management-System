# WorkSphere

WorkSphere is an employee management and workforce operations platform built with ASP.NET Core MVC.

The application provides role-based access for administrators and employees, with features for managing employee records, tasks, attendance, performance, company news, events, and an AI assistant.

## Features

- Role-based authentication and authorization
- Admin and employee dashboards
- Employee management
- Task management and task completion
- Attendance management
- Employee performance tracking
- Company news management
- Company events management
- Employee profile management
- AI assistant powered by Ollama
- SQL Server database integration
- Responsive web interface
- ASP.NET Core Identity authentication

## User Roles

### Administrator

Administrators can:

- Manage employee records
- Manage employee tasks
- Manage attendance records
- Manage performance records
- Manage company news
- Manage company events
- View workforce information
- Manage user and employee assignments

### Employee

Employees can:

- View their own dashboard
- View their profile
- View assigned tasks
- Complete assigned tasks
- View attendance information
- View performance information
- View company news
- View company events
- Use the AI assistant

## Technology Stack

- C#
- ASP.NET Core MVC
- .NET 10
- Entity Framework Core
- ASP.NET Core Identity
- SQL Server
- HTML
- CSS
- Bootstrap
- JavaScript
- Ollama
- Git

## Project Structure

```text
EmployeeManagementSystem/
├── Areas/
│   └── Identity/
├── Controllers/
├── Data/
├── Migrations/
├── Models/
├── Services/
├── Views/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── EmployeeManagementSystem.csproj
```

## Getting Started

For installation and setup instructions, see the [Getting Started Guide](GETTING_STARTED.md).
