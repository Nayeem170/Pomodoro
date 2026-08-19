using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Schedule;

public class ScheduleAgendaBase : ComponentBase
{
    [Parameter] public IReadOnlyList<ScheduleDay> Days { get; set; } = [];
    [Parameter] public string WindowLabel { get; set; } = string.Empty;
    [Parameter] public bool CanGoPrev { get; set; }
    [Parameter] public EventCallback OnPrev { get; set; }
    [Parameter] public EventCallback OnNext { get; set; }
    [Parameter] public EventCallback<TaskItem> OnEditTask { get; set; }

    [Parameter] public EventCallback<AddSubtaskRequest> OnAddSubtask { get; set; }
    [Parameter] public EventCallback<Guid> OnComplete { get; set; }
    [Parameter] public EventCallback<Guid> OnDelete { get; set; }
    [Parameter] public EventCallback<Guid> OnUncomplete { get; set; }
    [Parameter] public EventCallback<Guid> OnReparentToRoot { get; set; }
    [Parameter] public EventCallback<DemoteRequest> OnDemote { get; set; }
    [Parameter] public EventCallback<Guid> OnToggleFollowParent { get; set; }
    [Parameter] public EventCallback<Guid> OnSelect { get; set; }
    [Parameter] public EventCallback<ReorderRequest> OnReorder { get; set; }

    [Parameter] public IReadOnlyList<TaskItem> AllTasks { get; set; } = [];

    [Parameter] public IReadOnlyList<TaskListRef> GoogleLists { get; set; } = [];
}
