using ProjectManagement.DAL;
using TaskStatus = ProjectManagement.DAL.TaskStatus;

namespace ProjectManagement.BLL;

public static class TaskStatusTransitionService
{
    private static readonly Dictionary<TaskStatus, TaskStatus> AllowedTransitions = new()
    {
        { TaskStatus.Pending,    TaskStatus.InProgress },
        { TaskStatus.InProgress, TaskStatus.PRRaised   },
        { TaskStatus.PRRaised,   TaskStatus.Merged      },
        { TaskStatus.Merged,     TaskStatus.QAApproved  },
        { TaskStatus.QAApproved, TaskStatus.Deployed    },
        { TaskStatus.Deployed,   TaskStatus.Done        }
    };

    public static bool IsValid(TaskStatus current, TaskStatus next)
        => AllowedTransitions.TryGetValue(current, out var allowed) && allowed == next;

    public static TaskStatus GetNextStatus(TaskStatus current)
        => AllowedTransitions.TryGetValue(current, out var next)
            ? next
            : throw new BadRequestException($"No transition defined from status '{current}'.");
}
