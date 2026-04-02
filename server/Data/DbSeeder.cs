using ProjectManagement.Entities;
using ProjectManagement.Enums;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        // ── Users (30 total: 10 PM, 10 Developer, 10 QA) ──────────────────────
        var users = new List<User>();

        for (int i = 1; i <= 10; i++)
            users.Add(new User
            {
                Username = $"pm_user{i}",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword($"pm_pass{i}"),
                Role = UserRole.PM
            });

        for (int i = 1; i <= 10; i++)
            users.Add(new User
            {
                Username = $"dev_user{i}",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword($"dev_pass{i}"),
                Role = UserRole.Developer
            });

        for (int i = 1; i <= 10; i++)
            users.Add(new User
            {
                Username = $"qa_user{i}",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword($"qa_pass{i}"),
                Role = UserRole.QA
            });

        context.Users.AddRange(users);
        context.SaveChanges();

        // ── Releases (30 total) ────────────────────────────────────────────────
        var baseDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var releaseTitles = new[]
        {
            "Q1 Platform Rewrite", "Auth Module v2", "Dashboard Revamp", "Mobile API Layer",
            "Search Indexing", "Notification Service", "Payment Gateway", "Reporting Engine",
            "Admin Panel v3", "User Profile Overhaul", "Data Export Feature", "Bulk Import Tool",
            "Audit Trail", "Permission System", "Multi-tenancy Support", "Email Templates",
            "Analytics Pipeline", "Cache Layer", "File Upload Service", "Webhooks Integration",
            "OAuth Provider", "Two-Factor Auth", "Dark Mode Support", "Onboarding Flow",
            "Billing Module", "SLA Tracker", "API Rate Limiting", "Health Check Dashboard",
            "Disaster Recovery", "Performance Profiler"
        };

        var releaseStatuses = new[]
        {
            ReleaseStatus.Shipped, ReleaseStatus.Shipped, ReleaseStatus.Shipped, ReleaseStatus.Shipped,
            ReleaseStatus.Shipped, ReleaseStatus.Shipped, ReleaseStatus.Shipped, ReleaseStatus.Shipped,
            ReleaseStatus.Active,  ReleaseStatus.Active,  ReleaseStatus.Active,  ReleaseStatus.Active,
            ReleaseStatus.Active,  ReleaseStatus.Active,  ReleaseStatus.Active,  ReleaseStatus.Active,
            ReleaseStatus.Upcoming, ReleaseStatus.Upcoming, ReleaseStatus.Upcoming, ReleaseStatus.Upcoming,
            ReleaseStatus.Upcoming, ReleaseStatus.Upcoming, ReleaseStatus.Upcoming, ReleaseStatus.Upcoming,
            ReleaseStatus.Cancelled, ReleaseStatus.Cancelled, ReleaseStatus.Cancelled,
            ReleaseStatus.Cancelled, ReleaseStatus.Cancelled, ReleaseStatus.Cancelled
        };

        var releases = new List<Release>();
        for (int i = 0; i < 30; i++)
        {
            var startOffset = i * 14;
            releases.Add(new Release
            {
                Title = releaseTitles[i],
                Description = $"Full scope delivery for {releaseTitles[i]} including design, dev, and QA sign-off.",
                StartDate = baseDate.AddDays(startOffset),
                EndDate = baseDate.AddDays(startOffset + 13),
                Status = releaseStatuses[i],
                CreatedAt = baseDate.AddDays(startOffset - 7)
            });
        }

        context.Releases.AddRange(releases);
        context.SaveChanges();

        // ── Tasks (30 total spread across first 10 releases) ──────────────────
        var devUsers = users.Where(u => u.Role == UserRole.Developer).ToList();

        var taskTitles = new[]
        {
            "Setup project scaffolding", "Configure CI/CD pipeline", "Design DB schema",
            "Implement auth middleware", "Write unit tests for service layer", "Create API endpoints",
            "Build frontend components", "Integrate Swagger docs", "Performance load testing",
            "Code review and cleanup", "Fix login edge case bug", "Add pagination support",
            "Implement role-based access", "Refactor repository layer", "Add logging middleware",
            "Write integration tests", "Deploy to staging", "Update seed data",
            "Add error handling", "Implement caching strategy", "Create migration scripts",
            "Update OpenAPI spec", "Fix CORS configuration", "Add health check endpoint",
            "Implement soft delete", "Add audit log support", "Configure rate limiting",
            "Write deployment runbook", "Set up monitoring alerts", "Final QA sign-off"
        };

        var taskStatuses = new[]
        {
            TaskStatus.Done,       TaskStatus.Done,       TaskStatus.Done,       TaskStatus.Done,
            TaskStatus.Done,       TaskStatus.Done,       TaskStatus.Deployed,   TaskStatus.Deployed,
            TaskStatus.Deployed,   TaskStatus.Deployed,   TaskStatus.Merged,     TaskStatus.Merged,
            TaskStatus.Merged,     TaskStatus.Merged,     TaskStatus.PRRaised,   TaskStatus.PRRaised,
            TaskStatus.PRRaised,   TaskStatus.PRRaised,   TaskStatus.InProgress, TaskStatus.InProgress,
            TaskStatus.InProgress, TaskStatus.InProgress, TaskStatus.Pending,    TaskStatus.Pending,
            TaskStatus.Pending,    TaskStatus.Pending,    TaskStatus.Pending,    TaskStatus.Pending,
            TaskStatus.Pending,    TaskStatus.Pending
        };

        var prLinks = new[]
        {
            "https://github.com/org/repo/pull/101", "https://github.com/org/repo/pull/102",
            "https://github.com/org/repo/pull/103", "https://github.com/org/repo/pull/104",
            "https://github.com/org/repo/pull/105", "https://github.com/org/repo/pull/106",
            "https://github.com/org/repo/pull/107", "https://github.com/org/repo/pull/108",
            "https://github.com/org/repo/pull/109", "https://github.com/org/repo/pull/110",
            "https://github.com/org/repo/pull/111", "https://github.com/org/repo/pull/112",
            "https://github.com/org/repo/pull/113", "https://github.com/org/repo/pull/114",
            "https://github.com/org/repo/pull/115"
        };

        var tasks = new List<Entities.Task>();
        for (int i = 0; i < 30; i++)
        {
            var releaseIndex = i % 10;
            var devUser = devUsers[i % devUsers.Count];
            var status = taskStatuses[i];

            var task = new Entities.Task
            {
                Title = taskTitles[i],
                ReleaseId = releases[releaseIndex].Id,
                AssignedToUserId = devUser.Id,
                Status = status,
                CreatedAt = releases[releaseIndex].CreatedAt.AddDays(1 + (i % 5))
            };

            if (status >= TaskStatus.PRRaised && i < prLinks.Length)
                task.PRLink = prLinks[i];

            if (status >= TaskStatus.Done)
            {
                task.ApproverName = $"qa_user{(i % 10) + 1}";
                task.Remarks = "Verified and approved in QA environment.";
            }
            else if (status >= TaskStatus.Merged)
            {
                task.Remarks = "Merged to main. Awaiting deployment.";
            }

            tasks.Add(task);
        }

        context.Tasks.AddRange(tasks);
        context.SaveChanges();

        // ── Revoked Tokens (6 entries) ─────────────────────────────────────────
        var revokedTokens = new List<RevokedToken>
        {
            new() { Jti = Guid.NewGuid().ToString(), ExpiresAt = DateTime.UtcNow.AddDays(-1) },
            new() { Jti = Guid.NewGuid().ToString(), ExpiresAt = DateTime.UtcNow.AddDays(-2) },
            new() { Jti = Guid.NewGuid().ToString(), ExpiresAt = DateTime.UtcNow.AddDays(-3) },
            new() { Jti = Guid.NewGuid().ToString(), ExpiresAt = DateTime.UtcNow.AddDays(-5) },
            new() { Jti = Guid.NewGuid().ToString(), ExpiresAt = DateTime.UtcNow.AddDays(-7) },
            new() { Jti = Guid.NewGuid().ToString(), ExpiresAt = DateTime.UtcNow.AddDays(-10) }
        };

        context.RevokedTokens.AddRange(revokedTokens);
        context.SaveChanges();
    }
}
