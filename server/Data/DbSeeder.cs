using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Entities;
using ProjectManagement.Enums;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Data;

public static class DbSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        AppDbContext context)
    {
        if (await userManager.Users.AnyAsync())
            return;

        // ── Roles ─────────────────────────────────────────────────────────────
        foreach (var role in new[] { "PM", "Developer", "QA" })
            await roleManager.CreateAsync(new IdentityRole<int>(role));

        // ── Users ─────────────────────────────────────────────────────────────
        var pmUser = new User { UserName = "pm_user1" };
        await userManager.CreateAsync(pmUser, "pm_pass1");
        await userManager.AddToRoleAsync(pmUser, "PM");

        var devUsers = new List<User>();
        for (int i = 1; i <= 15; i++)
        {
            var dev = new User { UserName = $"dev_user{i}" };
            await userManager.CreateAsync(dev, $"dev_pass{i}");
            await userManager.AddToRoleAsync(dev, "Developer");
            devUsers.Add(dev);
        }

        var qaUsers = new List<User>();
        for (int i = 1; i <= 15; i++)
        {
            var qa = new User { UserName = $"qa_user{i}" };
            await userManager.CreateAsync(qa, $"qa_pass{i}");
            await userManager.AddToRoleAsync(qa, "QA");
            qaUsers.Add(qa);
        }

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
        await context.SaveChangesAsync();

        // ── Tasks (30 total spread across first 10 releases) ──────────────────
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
            TaskStatus.Deployed,   TaskStatus.Deployed,   TaskStatus.QAApproved, TaskStatus.QAApproved,
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
            var qaUser  = qaUsers[i % qaUsers.Count];
            var status  = taskStatuses[i];

            var task = new Entities.Task
            {
                Title             = taskTitles[i],
                ReleaseId         = releases[releaseIndex].Id,
                AssignedToUserId  = devUser.Id,
                AssignedToQAUserId = qaUser.Id,
                Status            = status,
                CreatedAt         = releases[releaseIndex].CreatedAt.AddDays(1 + (i % 5))
            };

            if (status >= TaskStatus.PRRaised && i < prLinks.Length)
                task.PRLink = prLinks[i];

            if (status >= TaskStatus.Done)
            {
                task.Remarks = "Verified and approved in QA environment.";
            }
            else if (status >= TaskStatus.QAApproved)
            {
                task.Remarks = "QA approved. Awaiting deployment.";
            }
            else if (status == TaskStatus.Merged)
            {
                task.Remarks = "Merged to main. Awaiting QA approval.";
            }

            tasks.Add(task);
        }

        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();
    }
}
