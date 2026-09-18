# Getting Started

This guide explains how to set up and run WorkSphere locally.

## Prerequisites

Make sure the following are installed:

- .NET 10 SDK
- SQL Server Express or SQL Server
- Visual Studio 2026
- Ollama, if you want to use the AI assistant

## 1. Clone the Repository

Clone the repository and open the project directory:

    git clone https://github.com/YOUR-USERNAME/Employee-Management-System.git
    cd Employee-Management-System

## 2. Configure the Database

Create a local file named:

    appsettings.Local.json

Add your local SQL Server connection string:

    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EmployeeManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
      }
    }

The `appsettings.Local.json` file is excluded from Git version control and should not be committed to the repository.

If your SQL Server instance uses a different server name, update the connection string accordingly.

## 3. Restore Dependencies

Open the project in Visual Studio or run:

    dotnet restore

## 4. Apply Database Migrations

Open the Package Manager Console in Visual Studio and run:

    Update-Database

This applies the Entity Framework Core migrations and creates the required database structure.

## 5. Configure the AI Assistant

WorkSphere uses Ollama for its local AI assistant.

Install Ollama and download the model:

    ollama pull llama3.2:3b

If Ollama is not already running, start it with:

    ollama serve

The AI assistant communicates with Ollama locally.

## 6. Build the Application

Build the project with:

    dotnet build

Make sure the project builds successfully without errors.

## 7. Run the Application

You can run the application from Visual Studio by pressing:

    F5

Or run it from the terminal:

    dotnet run

ASP.NET Core will start the application using its local development server.

Open the URL shown in the terminal or Visual Studio.

## 8. Create an Account

Open the application in your browser and register an account.

WorkSphere uses ASP.NET Core Identity for authentication and role-based authorization.

The application supports the following roles:

- Admin
- User

## 9. Admin Account

The application can automatically assign the Admin role to the account configured in `Program.cs`.

By default, the configured email is:

    admin@test.com

If you use a different account, update the `adminEmail` value in `Program.cs` before running the application.

For production deployments, use a secure account configuration and do not commit passwords or other sensitive credentials.

## 10. Using WorkSphere

After signing in:

### Administrators

Administrators can:

- Manage employees
- Manage tasks
- Manage attendance
- Manage performance records
- Manage company news
- Manage company events
- Manage users and employee assignments

### Employees

Employees can:

- View their dashboard
- View their profile
- View assigned tasks
- Complete assigned tasks
- View attendance information
- View performance information
- View company news
- View company events
- Use the AI assistant

## Troubleshooting

### Database Connection Error

Check that:

1. SQL Server is installed and running.
2. The server name in `appsettings.Local.json` is correct.
3. The `EmployeeManagementDB` database can be created by the configured SQL Server instance.

### AI Assistant Not Responding

Check that:

1. Ollama is installed.
2. Ollama is running.
3. The `llama3.2:3b` model is installed.

You can check the installed models with:

    ollama list

If necessary, download the model again:

    ollama pull llama3.2:3b

### Migration Error

Make sure the project is using the correct database connection and run:

    Update-Database

again from the Visual Studio Package Manager Console.

## Security

Do not commit the following to the repository:

- Database credentials
- API keys
- Passwords
- Personal access tokens
- Other sensitive configuration

Local configuration files such as `appsettings.Local.json` are excluded from version control through `.gitignore`.
