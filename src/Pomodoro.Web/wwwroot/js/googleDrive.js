window.googleDrive = {
    _tokenClient: null,
    _accessToken: null,
    _clientId: null,

    _waitForGis: function() {
        return new Promise((resolve, reject) => {
            if (window.google && window.google.accounts && window.google.accounts.oauth2) {
                resolve();
                return;
            }
            var attempts = 0;
            var maxAttempts = 50;
            var interval = setInterval(function() {
                attempts++;
                if (window.google && window.google.accounts && window.google.accounts.oauth2) {
                    clearInterval(interval);
                    resolve();
                } else if (attempts >= maxAttempts) {
                    clearInterval(interval);
                    reject(new Error('Google Identity Services script failed to load'));
                }
            }, 200);
        });
    },

    init: function(clientId) {
        return this._waitForGis().then(() => {
            this._clientId = clientId;
            this._tokenClient = google.accounts.oauth2.initTokenClient({
                client_id: clientId,
                scope: 'https://www.googleapis.com/auth/drive.appdata',
                callback: (response) => {
                    if (response.access_token) {
                        this._accessToken = response.access_token;
                    }
                }
            });
        });
    },

    requestAuth: function() {
        return new Promise((resolve, reject) => {
            if (!this._tokenClient) {
                reject(new Error('Google Drive not initialized. Call init() first.'));
                return;
            }
            this._tokenClient.callback = (response) => {
                if (response.error) {
                    reject(new Error(response.error));
                    return;
                }
                this._accessToken = response.access_token;
                resolve(response.access_token);
            };
            this._tokenClient.requestAccessToken();
        });
    },

    revokeAuth: function() {
        return new Promise((resolve) => {
            if (this._accessToken) {
                google.accounts.oauth2.revoke(this._accessToken, () => {
                    this._accessToken = null;
                    resolve();
                });
            } else {
                resolve();
            }
        });
    },

    isConnected: function() {
        return this._accessToken !== null && this._accessToken !== undefined;
    },

    _getAuthHeaders: function() {
        return {
            'Authorization': 'Bearer ' + this._accessToken
        };
    },

    findSyncFile: function(fileName) {
        const query = "name='" + fileName + "'";
        const url = 'https://www.googleapis.com/drive/v3/files?spaces=appDataFolder&q=' + encodeURIComponent(query) + '&fields=files(id,name,modifiedTime)';
        return fetch(url, { headers: this._getAuthHeaders() })
            .then(response => {
                if (response.status === 401) throw { status: 401, message: 'Unauthorized' };
                if (!response.ok) throw new Error('Failed to find sync file: ' + response.status);
                return response.json();
            })
            .then(data => {
                if (data.files && data.files.length > 0) {
                    return data.files[0].id;
                }
                return null;
            });
    },

    readFile: function(fileId) {
        const url = 'https://www.googleapis.com/drive/v3/files/' + fileId + '?alt=media';
        return fetch(url, { headers: this._getAuthHeaders() })
            .then(response => {
                if (response.status === 401) throw { status: 401, message: 'Unauthorized' };
                if (response.status === 404) throw { status: 404, message: 'File not found' };
                if (!response.ok) throw new Error('Failed to read file: ' + response.status);
                return response.text();
            });
    },

    createFile: function(fileName, content) {
        const boundary = '-------314159265358979323846';
        const delimiter = '\r\n--' + boundary + '\r\n';
        const closeDelimiter = '\r\n--' + boundary + '--';

        const metadata = JSON.stringify({
            name: fileName,
            parents: ['appDataFolder']
        });

        const requestBody =
            delimiter +
            'Content-Type: application/json; charset=UTF-8\r\n\r\n' +
            metadata +
            delimiter +
            'Content-Type: application/json; charset=UTF-8\r\n\r\n' +
            content +
            closeDelimiter;

        const url = 'https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart';
        return fetch(url, {
            method: 'POST',
            headers: {
                ...this._getAuthHeaders(),
                'Content-Type': 'multipart/related; boundary=' + boundary
            },
            body: requestBody
        })
            .then(response => {
                if (response.status === 401) throw { status: 401, message: 'Unauthorized' };
                if (!response.ok) throw new Error('Failed to create file: ' + response.status);
                return response.json();
            })
            .then(data => data.id);
    },

    updateFile: function(fileId, content) {
        const boundary = '-------314159265358979323846';
        const delimiter = '\r\n--' + boundary + '\r\n';
        const closeDelimiter = '\r\n--' + boundary + '--';

        const metadata = JSON.stringify({ name: 'pomodoro-sync.json' });

        const requestBody =
            delimiter +
            'Content-Type: application/json; charset=UTF-8\r\n\r\n' +
            metadata +
            delimiter +
            'Content-Type: application/json; charset=UTF-8\r\n\r\n' +
            content +
            closeDelimiter;

        const url = 'https://www.googleapis.com/upload/drive/v3/files/' + fileId + '?uploadType=multipart';
        return fetch(url, {
            method: 'PATCH',
            headers: {
                ...this._getAuthHeaders(),
                'Content-Type': 'multipart/related; boundary=' + boundary
            },
            body: requestBody
        })
            .then(response => {
                if (response.status === 401) throw { status: 401, message: 'Unauthorized' };
                if (!response.ok) throw new Error('Failed to update file: ' + response.status);
                return response.json();
            });
    },

    deleteFile: function(fileId) {
        const url = 'https://www.googleapis.com/drive/v3/files/' + fileId;
        return fetch(url, {
            method: 'DELETE',
            headers: this._getAuthHeaders()
        })
            .then(response => {
                if (response.status === 401) throw { status: 401, message: 'Unauthorized' };
                if (!response.ok) throw new Error('Failed to delete file: ' + response.status);
            });
    }
};
