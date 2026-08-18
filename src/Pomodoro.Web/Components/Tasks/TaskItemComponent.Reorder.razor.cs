using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Tasks;

public partial class TaskItemBase
{
    #region Parameters (Reorder)

    [Parameter]
    public bool IsReorderable { get; set; }

    [Parameter]
    public bool IsDragActive { get; set; }

    [Parameter]
    public Guid? DraggedTaskId { get; set; }

    [Parameter]
    public EventCallback<ReorderRequest> OnReorder { get; set; }

    [Parameter]
    public EventCallback<Guid> OnDragStarted { get; set; }

    [Parameter]
    public EventCallback OnDragEnded { get; set; }

    [Parameter]
    public IReadOnlyList<TaskItem> ReorderGroup { get; set; } = [];

    #endregion

    #region State (Reorder)

    protected bool _isDragSource;
    protected bool _dropBefore;
    protected bool _dropAfter;
    protected bool _noDropHover;

    protected bool CanDrag => IsReorderable && !IsInlineEditing;

    #endregion

    #region Reorder Methods

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (!IsDragActive) _noDropHover = false;
    }

    protected async Task HandleDragStart()
    {
        _isDragSource = true;
        await OnDragStarted.InvokeAsync(Item.Id);
    }

    protected async Task HandleDragEnd()
    {
        _isDragSource = false;
        _dropBefore = false;
        _dropAfter = false;
        _noDropHover = false;
        await OnDragEnded.InvokeAsync();
    }

    protected void HandleZoneDragOver(bool before)
    {
        _dropBefore = before;
        _dropAfter = !before;
    }

    protected void HandleZoneDragLeave()
    {
        _dropBefore = false;
        _dropAfter = false;
    }

    protected async Task HandleZoneDrop(bool before)
    {
        _dropBefore = false;
        _dropAfter = false;
        _isDragSource = false;
        if (DraggedTaskId.HasValue && DraggedTaskId.Value != Item.Id)
        {
            await OnReorder.InvokeAsync(new ReorderRequest(DraggedTaskId.Value, Item.Id, before));
        }
    }

    protected async Task HandleKeyboardReorder(bool up)
    {
        if (!IsReorderable || IsInlineEditing || ReorderGroup.Count < 2) return;

        var index = IndexOfSelf();
        if (index < 0) return;

        if (up)
        {
            if (index == 0) return;
            await OnReorder.InvokeAsync(new ReorderRequest(Item.Id, ReorderGroup[index - 1].Id, InsertBefore: true));
        }
        else
        {
            if (index == ReorderGroup.Count - 1) return;
            await OnReorder.InvokeAsync(new ReorderRequest(Item.Id, ReorderGroup[index + 1].Id, InsertBefore: false));
        }
    }

    private int IndexOfSelf()
    {
        for (var i = 0; i < ReorderGroup.Count; i++)
        {
            if (ReorderGroup[i].Id == Item.Id) return i;
        }
        return -1;
    }

    #endregion
}
