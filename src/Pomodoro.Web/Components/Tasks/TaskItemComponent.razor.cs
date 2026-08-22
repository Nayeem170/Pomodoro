using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Tasks;

public partial class TaskItemBase : ComponentBase
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
    protected ElementReference _demotePickerElement;
    protected ElementReference _demoteTriggerElement;

    private bool _shouldFocusInlineEdit;
    private bool _highlightScrolled;
    private bool _shouldScrollDemotePicker;
    private bool _shouldFocusFirstDemotePick;
    private bool _shouldFocusDemoteTrigger;
    protected int _focusedPickIndex;

    protected string DemotePickId(int index) => $"demote-pick-{Item.Id}-{index}";

    protected bool CanAddSubtask => Depth < Constants.Tasks.MaxSubtaskDepth;

    protected bool CanMoveToRoot => Depth > 0;

    protected bool CanDemote => Siblings is { Count: > 0 };

    [Parameter]
    public string? GoogleListTitle { get; set; }

    [Parameter]
    public IReadOnlyList<TaskListRef> GoogleLists { get; set; } = [];

    protected string GoogleBadgeTooltip =>
        string.IsNullOrEmpty(GoogleListTitle) ? "Google task" : $"Google task - {GoogleListTitle}";

    protected bool IsAddSubtaskDisabled => string.IsNullOrWhiteSpace(NewSubtaskName);

    protected string? RepeatLabelText => Item.Repeat?.Type switch
    {
        RepeatType.Daily => "Daily",
        RepeatType.Weekly => "Weekly",
        RepeatType.Custom => Item.Repeat.CustomDays > 0 ? $"×{Item.Repeat.CustomDays}d" : "Repeat",
        RepeatType.Monthly => "Monthly",
        RepeatType.Quarterly => "Quarterly",
        RepeatType.Yearly => "Yearly",
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
        if (_isDragSource) classes.Add("dragging");
        if (_dropBefore) classes.Add("drop-before");
        if (_dropAfter) classes.Add("drop-after");
        if (IsDragActive && _noDropHover && !IsReorderable) classes.Add("no-drop");
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
        var anchor = (Item.ScheduledDate ?? Item.Repeat.StartDate ?? Item.CreatedAt).Date;
        var typeLabel = Item.Repeat.Type switch
        {
            RepeatType.Daily => "Daily",
            RepeatType.Weekly => "Weekly",
            RepeatType.Custom => $"Every {Item.Repeat.CustomDays} days",
            RepeatType.Monthly => Item.Repeat.WeekOfMonth.HasValue
                ? $"Monthly ({WeekdayOfMonthLabel()})"
                : $"Monthly (day {Item.Repeat.MonthlyDay})",
            RepeatType.Quarterly => Item.Repeat.WeekOfMonth.HasValue
                ? $"Quarterly ({Constants.Repeat.QuarterlyGroupLabels[Item.Repeat.EffectiveQuarterGroup(anchor) - 1]}, {WeekdayOfMonthLabel()})"
                : $"Quarterly ({Constants.Repeat.QuarterlyGroupLabels[Item.Repeat.EffectiveQuarterGroup(anchor) - 1]}, day {Item.Repeat.QuarterlyDay ?? anchor.Day})",
            RepeatType.Yearly => Item.Repeat.WeekOfMonth.HasValue
                ? $"Yearly ({WeekdayOfMonthLabel()} of {Constants.Repeat.MonthNames[(Item.Repeat.YearlyMonth ?? anchor.Month) - 1]})"
                : $"Yearly (day {Item.Repeat.YearlyDay ?? anchor.Day} of {Constants.Repeat.MonthNames[(Item.Repeat.YearlyMonth ?? anchor.Month) - 1]})",
            _ => "Repeats"
        };
        if (Item.Repeat.IsPaused) return $"{typeLabel} (paused)";
        return typeLabel;
    }

    private string WeekdayOfMonthLabel()
    {
        var ordinal = Constants.Repeat.WeekOfMonthLabels[(Item.Repeat!.WeekOfMonth ?? 1) - 1].ToLowerInvariant();
        var days = string.Join(", ", Item.Repeat.Weekdays
            .OrderBy(d => d)
            .Select(d => d.ToString().Substring(0, 2)));
        return $"{ordinal} {days}";
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
        if (e.AltKey && e.Key == Constants.Keys.ArrowUp)
        {
            await HandleKeyboardReorder(up: true);
            return;
        }
        if (e.AltKey && e.Key == Constants.Keys.ArrowDown)
        {
            await HandleKeyboardReorder(up: false);
            return;
        }
        if (e.Key == "Enter" || e.Key == " ")
        {
            await HandleSelect();
        }
    }

    protected void HandleEdit()
    {
        IsEditing = !IsEditing;
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
        if (IsDemoteMenuOpen)
        {
            _shouldScrollDemotePicker = true;
            _shouldFocusFirstDemotePick = true;
            _focusedPickIndex = 0;
        }
    }

    protected async Task ConfirmDemote(Guid siblingId)
    {
        IsDemoteMenuOpen = false;
        await OnDemote.InvokeAsync(new DemoteRequest(Item.Id, siblingId));
    }

    protected void CancelDemote()
    {
        IsDemoteMenuOpen = false;
        _shouldFocusDemoteTrigger = true;
    }

    protected async Task HandlePickerKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == Constants.Keys.Escape)
        {
            CancelDemote();
        }
        else if (e.Key == Constants.Keys.ArrowDown && Siblings.Count > 0)
        {
            _focusedPickIndex = Math.Min(_focusedPickIndex + 1, Siblings.Count - 1);
            await FocusDemotePick(_focusedPickIndex);
        }
        else if (e.Key == Constants.Keys.ArrowUp && Siblings.Count > 0)
        {
            _focusedPickIndex = Math.Max(_focusedPickIndex - 1, 0);
            await FocusDemotePick(_focusedPickIndex);
        }
    }

    private async Task FocusDemotePick(int index)
    {
        try { await JSRuntime.InvokeVoidAsync("taskScrollInterop.focusElement", DemotePickId(index)); }
        catch (JSDisconnectedException) { }
        catch (JSException) { }
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

        if (_shouldFocusFirstDemotePick && Siblings.Count > 0)
        {
            _shouldFocusFirstDemotePick = false;
            await FocusDemotePick(0);
        }

        if (_shouldScrollDemotePicker)
        {
            _shouldScrollDemotePicker = false;
            try
            {
                await JSRuntime.InvokeVoidAsync("taskScrollInterop.scrollIntoViewIfNeeded", _demotePickerElement);
            }
            catch (JSDisconnectedException) { }
        }

        if (_shouldFocusDemoteTrigger)
        {
            _shouldFocusDemoteTrigger = false;
            try { await _demoteTriggerElement.FocusAsync(); }
            catch (JSDisconnectedException) { }
            catch (JSException) { }
            catch (InvalidOperationException) { }
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
