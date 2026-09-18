using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class AiAssistantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WorkSphereAiService _aiService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AiAssistantController(
            IHttpClientFactory httpClientFactory,
            WorkSphereAiService aiService,
            UserManager<ApplicationUser> userManager)
        {
            _httpClientFactory = httpClientFactory;
            _aiService = aiService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task Ask(string message)
        {
            Response.ContentType = "application/x-ndjson";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            if (string.IsNullOrWhiteSpace(message))
            {
                await WriteResponseAsync(new
                {
                    error = "Please enter a question."
                });

                return;
            }

            try
            {
                // =====================================================
                // GET CURRENT LOGGED-IN USER
                // =====================================================

                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                {
                    await WriteResponseAsync(new
                    {
                        error = "Your session could not be identified. Please log in again."
                    });

                    return;
                }

                bool isAdmin = await _userManager.IsInRoleAsync(
                    currentUser,
                    "Admin");

                int? employeeId = currentUser.EmployeeId;


                // =====================================================
                // GET LIVE WORKSPHERE DATA
                // =====================================================

                string liveData = await GetRelevantLiveDataAsync(
                    message,
                    employeeId,
                    isAdmin);

                bool hasLiveData = !liveData.StartsWith(
                    "NO_LIVE_DATA_REQUESTED",
                    StringComparison.OrdinalIgnoreCase);


                // =====================================================
// BUILD AI PROMPT
// =====================================================

string prompt;

if (hasLiveData)
{
    // =================================================
    // LIVE DATA QUESTION
    // =================================================

    prompt = $"""
        You are the WorkSphere Assistant.

        Answer the user's question using ONLY the LIVE DATA below.

        IMPORTANT:
        - The LIVE DATA is real data retrieved from the WorkSphere database.
        - The LIVE DATA is the source of truth.
        - Do not invent information.
        - Do not add metrics that are not in the LIVE DATA.
        - Do not tell the user to visit a page, tab, dashboard, or profile.
        - Do not give navigation instructions.
        - Do not replace the answer with generic WorkSphere information.
        - Answer the question directly.
        - If the user asks for history, list the actual records.
        - Keep averages separate from individual records.
        - If there are no records, say that there are no records.
        - Never reveal internal EmployeeId values.

        LIVE DATA
        ================================
        {liveData}
        ================================

        USER QUESTION
        ================================
        {message}
        ================================

        Give only the answer to the user's question.
        """;
}
else
{
    // =================================================
    // GENERAL WORKSPHERE QUESTION
    // =================================================

    prompt = $"""
        You are the WorkSphere AI Assistant.

        WorkSphere is an employee management and workforce
        operations platform.

        WorkSphere contains:
        - Dashboard
        - Employees
        - Tasks
        - Company News
        - Company Events
        - Attendance
        - Performance
        - Profile
        - Authentication
        - AI Assistant

        Administrators can manage employee records, tasks,
        news, events, attendance, and performance.

        Employees can view their own information, tasks,
        attendance, performance, news, and events.

        Answer the user's question naturally and directly.

        If the user asks how to find or access something,
        provide navigation instructions.

        If the user asks about permissions, explain the
        relevant employee or administrator permissions.

        Do not invent private employee information.

        USER QUESTION
        ================================
        {message}
        ================================

        Give a concise and helpful answer.
        """;
}


                // =====================================================
                // CALL OLLAMA
                // =====================================================

                var client = _httpClientFactory.CreateClient();

                var requestBody = new
                {
                    model = "llama3.2:3b",

                    prompt,

                    stream = true,

                    keep_alive = "30m",

                    options = new
                    {
                        temperature = 0.0,
                        num_predict = 160,
                        num_ctx = 2048
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "http://localhost:11434/api/generate");

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    await WriteResponseAsync(new
                    {
                        error = "The local AI service returned an error."
                    });

                    return;
                }

                await using var responseStream =
                    await response.Content.ReadAsStreamAsync();

                using var reader =
                    new StreamReader(responseStream);

                // =====================================================
                // STREAM OLLAMA RESPONSE
                // =====================================================

                string? line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    await Response.WriteAsync(line + "\n");
                    await Response.Body.FlushAsync();
                }
            }
            catch (HttpRequestException)
            {
                await WriteResponseAsync(new
                {
                    error = "The local AI service is not running. Please make sure Ollama is running."
                });
            }
            catch (OperationCanceledException)
            {
                // Request was cancelled by the client.
            }
            catch (Exception)
            {
                await WriteResponseAsync(new
                {
                    error = "Sorry, the WorkSphere Assistant is temporarily unavailable."
                });
            }
        }


        // =============================================================
        // GET RELEVANT LIVE WORKSPHERE DATA
        // =============================================================

        private async Task<string> GetRelevantLiveDataAsync(
            string message,
            int? employeeId,
            bool isAdmin)
        {
            string question = message.ToLowerInvariant();

            var data = new StringBuilder();

            bool dataRequested = false;


            // =========================================================
            // EMPLOYEE TASKS
            // =========================================================

            if (ContainsAny(
                question,
                "task",
                "tasks",
                "assigned",
                "assignment",
                "assignments",
                "work assigned",
                "unfinished",
                "pending work",
                "my work"))
            {
                dataRequested = true;

                if (!employeeId.HasValue)
                {
                    data.AppendLine(
                        "The current user is not linked to an employee record.");
                }
                else
                {
                    data.AppendLine(
                        await _aiService.GetMyTasksAsync(
                            employeeId.Value));
                }
            }


            // =========================================================
            // EMPLOYEE ATTENDANCE
            // =========================================================

            if (ContainsAny(
                question,
                "attendance",
                "attend",
                "present",
                "absent",
                "late",
                "attendance percentage",
                "attendance history",
                "attendance record",
                "attendance records"))
            {
                dataRequested = true;

                if (!employeeId.HasValue)
                {
                    data.AppendLine(
                        "The current user is not linked to an employee record.");
                }
                else
                {
                    data.AppendLine(
                        await _aiService.GetMyAttendanceAsync(
                            employeeId.Value));
                }
            }


            // =========================================================
            // EMPLOYEE PERFORMANCE
            // =========================================================

            if (ContainsAny(
                question,
                "performance",
                "performance score",
                "performance history",
                "performance record",
                "performance records",
                "my score",
                "my rating",
                "rating",
                "performance review",
                "performance reviews"))
            {
                dataRequested = true;

                if (!employeeId.HasValue)
                {
                    data.AppendLine(
                        "The current user is not linked to an employee record.");
                }
                else
                {
                    data.AppendLine(
                        await _aiService.GetMyPerformanceAsync(
                            employeeId.Value));
                }
            }


            // =========================================================
            // COMPANY EVENTS
            // =========================================================

            if (ContainsAny(
                question,
                "event",
                "events",
                "upcoming event",
                "upcoming events",
                "company event",
                "company events",
                "calendar"))
            {
                dataRequested = true;

                data.AppendLine(
                    await _aiService.GetUpcomingEventsAsync());
            }


            // =========================================================
            // COMPANY NEWS
            // =========================================================

            if (ContainsAny(
                question,
                "news",
                "announcement",
                "announcements",
                "company news",
                "latest news"))
            {
                dataRequested = true;

                data.AppendLine(
                    await _aiService.GetLatestNewsAsync());
            }


            // =========================================================
            // ADMIN EMPLOYEE COUNT
            // =========================================================

            if (isAdmin &&
                ContainsAny(
                    question,
                    "how many employees",
                    "employee count",
                    "number of employees",
                    "total employees",
                    "how many people work here",
                    "how many staff"))
            {
                dataRequested = true;

                data.AppendLine(
                    await _aiService.GetEmployeeCountAsync());
            }


            // =========================================================
            // ADMIN TASK SUMMARY
            // =========================================================

            if (isAdmin &&
                ContainsAny(
                    question,
                    "task summary",
                    "overall tasks",
                    "total tasks",
                    "completed tasks",
                    "pending tasks",
                    "task statistics",
                    "task overview"))
            {
                dataRequested = true;

                data.AppendLine(
                    await _aiService.GetTaskSummaryAsync());
            }


            // =========================================================
            // NO LIVE DATA REQUEST
            // =========================================================

            if (!dataRequested)
            {
                return """
                    NO_LIVE_DATA_REQUESTED

                    No live database data was requested for this question.

                    Use the WorkSphere application knowledge from the prompt.

                    Do not invent private employee information.
                    """;
            }

            return data.ToString();
        }


        // =============================================================
        // KEYWORD / INTENT HELPER
        // =============================================================

        private static bool ContainsAny(
            string text,
            params string[] values)
        {
            foreach (var value in values)
            {
                if (text.Contains(
                    value,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        // =============================================================
        // STREAMING ERROR RESPONSE
        // =============================================================

        private async Task WriteResponseAsync(object response)
        {
            await Response.WriteAsync(
                JsonSerializer.Serialize(response) + "\n");

            await Response.Body.FlushAsync();
        }
    }
}