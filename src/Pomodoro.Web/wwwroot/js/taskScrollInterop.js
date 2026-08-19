window.taskScrollInterop = {
    scrollIntoViewIfNeeded: function (element) {
        if (!element) return;
        var rect = element.getBoundingClientRect();
        var container = element.closest('.task-items') || element.parentElement;
        if (!container) {
            element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            return;
        }
        var cr = container.getBoundingClientRect();
        if (rect.top < cr.top || rect.bottom > cr.bottom) {
            element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    }
};
