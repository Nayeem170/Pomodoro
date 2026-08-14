using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Tasks;

public class TaskItemBase : ComponentBase
{
    #region Parameters (Model)

    [Parameter]
    public TaskItem Item { get; set; } = default!;

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public EventCallback<Guid> OnSelect { get; set; }

    [Parameter]
    public EventCallback<Guid> OnComplete { get; set; }

    [Parameter]
    public EventCallback<Guid> OnDelete { get; set; }

    [Parameter]
    public EventCallback<Guid> OnUncomplete { get; set; }

    [Parameter]
    public EventCallback<TaskItem> OnEdit { get; set; }

    [Parameter]
    public int Depth { get; set; }

    [Parameter]
    public EventCallback<AddSubtaskRequest> OnAddSubtask { get; set; }

    [Parameter]
    public EventCallback<Guid> OnReparentToRoot { get; set; }

    [Parameter]
    public EventCallback<DemoteRequest> OnDemote { get; set; }

    [Parameter]
    public EventCallback<Guid> OnToggleFollowParent { get; set; }

    [Parameter]
    public IReadOnlyList<TaskItem> Siblings { get; set; } = [];

    [Parameter]
    public bool HasChildren { get; set; }

    [Parameter]
    public int ChildCount { get; set; }

    [Parameter]
    public bool IsCollapsed { get; set; }

    [Parameter]
    public EventCallback<Guid> OnToggleCollapse { get; set; }

    [Parameter]
    public DateTime? ContextDate { get; set; }

    [Parameter]
    public bool IsNewlyAdded { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    #endregion

    #region State

    protected bool IsEditing { get; set; }

    protected bool IsInlineEditing { get; set; }

    protected bool IsAddingSubtask { get; set; }

    protected bool IsConfirmingDelete { get; set; }

    protected bool IsDemoteMenuOpen { get; set; }

    protected string NewSubtaskName { get; set; } = string.Empty;

    protected string InlineEditName { get; set; } = string.Empty;

    protected ElementReference _inlineEditInput;
    protected ElementReference _rowElement;

    private bool _shouldFocusInlineEdit;
    private bool _highlightScrolled;

    protected bool CanAddSubtask => Depth < Constants.Tasks.MaxSubtaskDepth;

    protected bool CanMoveToRoot => Depth > 0;

    protected bool CanDemote => Siblings is { Count: > 0 };

    [Parameter]
    public string? GoogleListTitle { get; set; }

    protected string GoogleBadgeTooltip =>
        string.IsNullOrEmpty(GoogleListTitle) ? "Google task" : $"Google task - {GoogleListTitle}";

    protected bool IsAddSubtaskDisabled => string.IsNullOrWhiteSpace(NewSubtaskName);

    protected string? RepeatLabelText => Item.Repeat?.Type switch
    {
        RepeatType.Daily => "Daily",
        RepeatType.Weekly => "Weekly",
        RepeatType.Custom => Item.Repeat.CustomDays > 0 ? $"×{Item.Repeat.CustomDays}d" : "Repeat",
        RepeatType.Monthly => "Monthly",
        _ => null
    };

    #endregion

    #region Business Logic Methods

    protected string FormatTime(int minutes)
    {
        if (minutes < Constants.TimeConversion.MinutesPerHour)
            return string.Format(Constants.TimeFormats.MinutesFormat, minutes);
        var hours = minutes / Constants.TimeConversion.MinutesPerHour;
        var mins = minutes % Constants.TimeConversion.MinutesPerHour;
        return string.Format(Constants.TimeFormats.HoursMinutesFormat, hours, mins);
    }

    protected string GetTaskClass()
    {
        var classes = new List<string>();
        if (IsSelected) classes.Add(Constants.Tasks.SelectedClass);
        if (Item.IsCompleted) classes.Add(Constants.Tasks.CompletedClass);
        if (IsNewlyAdded) classes.Add(Constants.Tasks.NewlyAddedClass);
        if (IsInlineEditing || IsDemoteMenuOpen) classes.Add("active-form");
        return string.Join(" ", classes);
    }

    protected string GetStatusIcon()
    {
        if (Item.IsCompleted) return Constants.Tasks.CompletedEmoji;
        if (Item.PomodoroCount > 0) return Constants.Tasks.HasPomodorosEmoji;
        return Constants.Tasks.DefaultEmoji;
    }

    protected string GetRepeatTooltip()
    {
        if (Item.Repeat == null) return string.Empty;
        var typeLabel = Item.Repeat.Type switch
        {
            RepeatType.Daily => "Daily",
            RepeatType.Weekly => "Weekly",
            RepeatType.Custom => $"Every {Item.Repeat.CustomDays} days",
            RepeatType.Monthly => $"Monthly (day {Item.Repeat.MonthlyDay})",
            _ => "Repeats"
        };
        if (Item.Repeat.IsPaused) return $"{typeLabel} (paused)";
        return typeLabel;
    }

    protected async Task HandleSelect()
    {
        if (!Item.IsCompleted)
        {
            await OnSelect.InvokeAsync(Item.Id);
        }
    }

    protected async Task HandleComplete()
    {
        await OnComplete.InvokeAsync(Item.Id);
    }

    protected async Task HandleDelete()
    {
        if (HasChildren)
        {
            IsConfirmingDelete = true;
            return;
        }
        await OnDelete.InvokeAsync(Item.Id);
    }

    protected void CancelDelete()
    {
        IsConfirmingDelete = false;
    }

    protected async Task ConfirmDelete()
    {
        IsConfirmingDelete = false;
        await OnDelete.InvokeAsync(Item.Id);
    }

    protected async Task HandleUncomplete()
    {
        await OnUncomplete.InvokeAsync(Item.Id);
    }

    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Key == " ")
        {
            await HandleSelect();
        }
    }

    protected void HandleEdit()
    {
        if (Depth == 0)
        {
            IsEditing = !IsEditing;
        }
        else
        {
            StartInlineEdit();
        }
    }

    protected void StartInlineEdit()
    {
        InlineEditName = Item.Name;
        IsInlineEditing = true;
        _shouldFocusInlineEdit = true;
    }

    protected async Task SaveInlineEdit()
    {
        if (!IsInlineEditing) return;
        var trimmed = (InlineEditName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            CancelInlineEdit();
            return;
        }
        IsInlineEditing = false;
        Item.Name = trimmed;
        await OnEdit.InvokeAsync(Item);
    }

    protected void CancelInlineEdit()
    {
        IsInlineEditing = false;
    }

    protected async Task HandleInlineEditKey(KeyboardEventArgs e)
    {
        if (e.Key == Constants.Keys.Enter)
            await SaveInlineEdit();
        else if (e.Key == Constants.Keys.Escape)
            CancelInlineEdit();
    }

    protected async Task HandleEditSave(TaskItem updatedTask)
    {
        await OnEdit.InvokeAsync(updatedTask);
        IsEditing = false;
    }

    protected void HandleEditCancel()
    {
        IsEditing = false;
    }

    protected void StartAddSubtask()
    {
        IsAddingSubtask = true;
        NewSubtaskName = string.Empty;
    }

    protected void CancelAddSubtask()
    {
        IsAddingSubtask = false;
        NewSubtaskName = string.Empty;
    }

    protected async Task HandleAddSubtask()
    {
        if (!string.IsNullOrWhiteSpace(NewSubtaskName))
        {
            await OnAddSubtask.InvokeAsync(new AddSubtaskRequest(Item.Id, NewSubtaskName.Trim()));
            NewSubtaskName = string.Empty;
            IsAddingSubtask = false;
        }
    }

    protected async Task HandleSubtaskKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == Constants.Keys.Enter && !string.IsNullOrWhiteSpace(NewSubtaskName))
        {
            await HandleAddSubtask();
        }
        else if (e.Key == Constants.Keys.Escape)
        {
            CancelAddSubtask();
        }
    }

    protected async Task HandleReparentToRoot()
    {
        await OnReparentToRoot.InvokeAsync(Item.Id);
    }

    protected async Task HandleToggleFollowParent()
    {
        await OnToggleFollowParent.InvokeAsync(Item.Id);
    }

    protected void HandleDemote()
    {
        IsDemoteMenuOpen = !IsDemoteMenuOpen;
    }

    protected async Task ConfirmDemote(Guid siblingId)
    {
        IsDemoteMenuOpen = false;
        await OnDemote.InvokeAsync(new DemoteRequest(Item.Id, siblingId));
    }

    protected void CancelDemote()
    {
        IsDemoteMenuOpen = false;
    }

    protected async Task HandleToggleCollapse()
    {
        await OnToggleCollapse.InvokeAsync(Item.Id);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldFocusInlineEdit)
        {
            _shouldFocusInlineEdit = false;
            try { await _inlineEditInput.FocusAsync(); } catch { }
        }

        if (IsNewlyAdded && !_highlightScrolled)
        {
            _highlightScrolled = true;
            try
            {
                await JSRuntime.InvokeVoidAsync("taskScrollInterop.scrollIntoViewIfNeeded", _rowElement);
            }
            catch (JSDisconnectedException) { }
        }
        else if (!IsNewlyAdded)
        {
            _highlightScrolled = false;
        }
    }

    #endregion
}
