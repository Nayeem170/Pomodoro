window.googleTasks = {
    _getAuthHeaders: function(accessToken) {
        return {
            'Authorization': 'Bearer ' + accessToken,
            'Content-Type': 'application/json'
        };
    },

    _getBaseUrl: function() {
        return 'https://tasks.googleapis.com/tasks/v1';
    },

    listTaskLists: function(accessToken) {
        var self = this;
        var url = self._getBaseUrl() + '/users/@me/lists';
        return self._fetchAllPages(url, accessToken).then(function(items) {
            return { items: items };
        }).then(function(data) { return JSON.stringify(data); });
    },

    listTasks: function(accessToken, listId, options) {
        var self = this;
        var params = 'showCompleted=true&showHidden=true';
        if (options && options.showDeleted) params += '&showDeleted=true';
        if (options && options.updatedMin) params += '&updatedMin=' + encodeURIComponent(options.updatedMin);
        if (options && options.pageToken) params += '&pageToken=' + encodeURIComponent(options.pageToken);
        var url = self._getBaseUrl() + '/lists/' + encodeURIComponent(listId) + '/tasks?' + params;
        return self._fetchAllPages(url, accessToken).then(function(items) {
            return JSON.stringify(items);
        });
    },

    insertTask: function(accessToken, listId, task) {
        var url = this._getBaseUrl() + '/lists/' + encodeURIComponent(listId) + '/tasks';
        return fetch(url, {
            method: 'POST',
            headers: this._getAuthHeaders(accessToken),
            body: JSON.stringify(task)
        })
            .then(function(response) {
                if (!response.ok) throw new Error('Failed to insert task: ' + response.status);
                return response.json();
            })
            .then(function(data) { return JSON.stringify(data); });
    },

    patchTask: function(accessToken, listId, taskId, updates) {
        var url = this._getBaseUrl() + '/lists/' + encodeURIComponent(listId) + '/tasks/' + encodeURIComponent(taskId);
        return fetch(url, {
            method: 'PATCH',
            headers: this._getAuthHeaders(accessToken),
            body: JSON.stringify(updates)
        })
            .then(function(response) {
                if (response.ok) return response.json();
                if (response.status === 404) throw new Error('404 Not Found');
                throw new Error('Failed to patch task: ' + response.status);
            })
            .then(function(data) { return JSON.stringify(data); });
    },

    deleteTask: function(accessToken, listId, taskId) {
        var url = this._getBaseUrl() + '/lists/' + encodeURIComponent(listId) + '/tasks/' + encodeURIComponent(taskId);
        return fetch(url, {
            method: 'DELETE',
            headers: this._getAuthHeaders(accessToken)
        })
            .then(function(response) {
                if (!response.ok) throw new Error('Failed to delete task: ' + response.status);
            });
    },

    _fetchAllPages: function(url, accessToken) {
        var self = this;
        return fetch(url, { headers: self._getAuthHeaders(accessToken) })
            .then(function(response) {
                if (response.status === 401) throw new Error('401 Unauthorized');
                if (response.status === 403) throw new Error('403 Forbidden');
                if (!response.ok) throw new Error('API request failed: ' + response.status);
                return response.json();
            })
            .then(function(data) {
                var items = data.items || data || [];
                if (data.nextPageToken) {
                    var separator = url.indexOf('?') >= 0 ? '&' : '?';
                    var nextUrl = url + separator + 'pageToken=' + encodeURIComponent(data.nextPageToken);
                    return self._fetchAllPages(nextUrl, accessToken).then(function(more) {
                        return items.concat(more);
                    });
                }
                return items;
            });
    }
};
