using ProjectManagement.Enums;
using ProjectManagement.Exceptions;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Services;

public static class TaskStatusTransitionService
{
    // Maps each status to the one and only valid next status
    private static readonly Dictionary<TaskStatus, TaskStatus> AllowedTransitions = new()
    {
        { TaskStatus.Pending,    TaskStatus.InProgress },
        { TaskStatus.InProgress, TaskStatus.PRRaised   },
        { TaskStatus.PRRaised,   TaskStatus.Merged      },
        { TaskStatus.Merged,     TaskStatus.Deployed    },
        { TaskStatus.Deployed,   TaskStatus.Done        }
    };

    public static bool IsValid(TaskStatus current, TaskStatus next)
        => AllowedTransitions.TryGetValue(current, out var allowed) && allowed == next;

    public static TaskStatus GetNextStatus(TaskStatus current)
        => AllowedTransitions.TryGetValue(current, out var next)
            ? next
            : throw new BadRequestException($"No transition defined from status '{current}'.");
}
