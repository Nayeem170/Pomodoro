// IndexedDB Interop for Blazor WebAssembly
// Provides persistent storage with larger capacity and better querying than localStorage

window.indexedDbInterop = {
    db: null,
    
    // Get database name from constants
    getDbName: function() {
        return pomodoroConstants.storage.dbName;
    },
    
    // Get database version from constants
    getDbVersion: function() {
        return pomodoroConstants.storage.dbVersion;
    },

    // Initialize database with schema
    initDatabase: async function() {
        return new Promise((resolve, reject) => {
            if (this.db) {
                resolve(true);
                return;
            }

            const request = indexedDB.open(this.getDbName(), this.getDbVersion());
            
            request.onerror = () => {
                console.error(pomodoroConstants.messages.indexedDbOpenFailed, request.error);
                reject(request.error);
            };
            
            request.onsuccess = () => {
                this.db = request.result;
                resolve(true);
            };
            
            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                const storage = pomodoroConstants.storage;
                const indexes = storage.indexes;
                
                // Tasks store
                if (!db.objectStoreNames.contains(storage.tasksStore)) {
                    const taskStore = db.createObjectStore(storage.tasksStore, { keyPath: indexes.id });
                    taskStore.createIndex(indexes.name, indexes.name, { unique: false });
                    taskStore.createIndex(indexes.createdAt, indexes.createdAt, { unique: false });
                    taskStore.createIndex(indexes.isCompleted, indexes.isCompleted, { unique: false });
                    taskStore.createIndex(indexes.lastWorkedOn, indexes.lastWorkedOn, { unique: false });
                    taskStore.createIndex(indexes.isDeleted, indexes.isDeleted, { unique: false });
                }
                
                // Activities store
                if (!db.objectStoreNames.contains(storage.activitiesStore)) {
                    const activityStore = db.createObjectStore(storage.activitiesStore, { keyPath: indexes.id });
                    activityStore.createIndex(indexes.type, indexes.type, { unique: false });
                    activityStore.createIndex(indexes.taskId, indexes.taskId, { unique: false });
                    activityStore.createIndex(indexes.completedAt, indexes.completedAt, { unique: false });
                }
                
                // Daily stats store
                if (!db.objectStoreNames.contains(storage.dailyStatsStore)) {
                    db.createObjectStore(storage.dailyStatsStore, { keyPath: indexes.date });
                }
                
                // Settings store
                if (!db.objectStoreNames.contains(storage.settingsStore)) {
                    db.createObjectStore(storage.settingsStore, { keyPath: indexes.id });
                }
                
                // App state store
                if (!db.objectStoreNames.contains(storage.appStateStore)) {
                    db.createObjectStore(storage.appStateStore, { keyPath: indexes.id });
                }
            };
        });
    },

    // Ensure database is initialized before operations
    ensureDb: async function() {
        if (!this.db) {
            await this.initDatabase();
        }
        return this.db;
    },

    // Add or update item
    putItem: async function(storeName, item) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readwrite');
                const store = transaction.objectStore(storeName);
                const request = store.put(item);
                
                request.onsuccess = () => resolve(true);
                request.onerror = () => {
                    console.error(`${pomodoroConstants.messages.indexedDbErrorPuttingItem} ${storeName}:`, request.error);
                    reject(request.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionPutItem, error);
                reject(error);
            }
        });
    },

    // Get single item by key
    getItem: async function(storeName, key) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readonly');
                const store = transaction.objectStore(storeName);
                const request = store.get(key);
                
                request.onsuccess = () => resolve(request.result || null);
                request.onerror = () => {
                    console.error(`${pomodoroConstants.messages.indexedDbErrorGettingItem} ${storeName}:`, request.error);
                    reject(request.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionGetItem, error);
                reject(error);
            }
        });
    },

    // Get all items from store
    getAllItems: async function(storeName) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readonly');
                const store = transaction.objectStore(storeName);
                const request = store.getAll();
                
                request.onsuccess = () => resolve(request.result || []);
                request.onerror = () => {
                    console.error(`${pomodoroConstants.messages.indexedDbErrorGettingAllItems} ${storeName}:`, request.error);
                    reject(request.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionGetAllItems, error);
                reject(error);
            }
        });
    },

    // Query by index
    getItemsByIndex: async function(storeName, indexName, value) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readonly');
                const store = transaction.objectStore(storeName);
                const index = store.index(indexName);
                const request = index.getAll(value);
                
                request.onsuccess = () => resolve(request.result || []);
                request.onerror = () => {
                    console.error(`${pomodoroConstants.messages.indexedDbErrorQueryingIndex} ${indexName}:`, request.error);
                    reject(request.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionGetItemsByIndex, error);
                reject(error);
            }
        });
    },

    // Get items in date range
    getItemsByDateRange: async function(storeName, indexName, startDate, endDate) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readonly');
                const store = transaction.objectStore(storeName);
                const index = store.index(indexName);
                const range = IDBKeyRange.bound(startDate, endDate);
                const request = index.getAll(range);
                
                request.onsuccess = () => resolve(request.result || []);
                request.onerror = () => {
                    console.error(pomodoroConstants.messages.indexedDbErrorQueryingDateRange, request.error);
                    reject(request.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionGetItemsByDateRange, error);
                reject(error);
            }
        });
    },

    // Delete item
    deleteItem: async function(storeName, key) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readwrite');
                const store = transaction.objectStore(storeName);
                const request = store.delete(key);
                
                request.onsuccess = () => resolve(true);
                request.onerror = () => {
                    console.error(`${pomodoroConstants.messages.indexedDbErrorDeletingItem} ${storeName}:`, request.error);
                    reject(request.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionDeleteItem, error);
                reject(error);
            }
        });
    },

    // Add or update all items in a single transaction (batch operation)
    // Returns an object with successCount and errorCount for proper error tracking
    putAllItems: async function(storeName, items) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readwrite');
                const store = transaction.objectStore(storeName);
                
                // Use a single transaction for all items
                let successCount = 0;
                let errorCount = 0;
                const errors = [];
                
                for (const item of items) {
                    const request = store.put(item);
                    request.onsuccess = () => successCount++;
                    request.onerror = (event) => {
                        errorCount++;
                        errors.push({
                            item: item,
                            error: event.target.error
                        });
                    };
                }
                
                transaction.oncomplete = () => {
                    if (errorCount > 0) {
                        console.warn(`${pomodoroConstants.messages.indexedDbBatchPutCompletedWithErrors} ${errorCount} errors`, errors);
                    }
                    // Return result object so caller can handle partial failures
                    resolve({ successCount, errorCount, errors });
                };
                
                transaction.onerror = () => {
                    console.error(pomodoroConstants.messages.indexedDbTransactionError, transaction.error);
                    reject(transaction.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionPutAllItems, error);
                reject(error);
            }
        });
    },

    // Clear all items from store
    clearStore: async function(storeName) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readwrite');
                const store = transaction.objectStore(storeName);
                const request = store.clear();
                
                request.onsuccess = () => resolve(true);
                request.onerror = () => {
                    console.error(`${pomodoroConstants.messages.indexedDbErrorClearingStore} ${storeName}:`, request.error);
                    reject(request.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionClearStore, error);
                reject(error);
            }
        });
    },

    // Get count of items in store
    getCount: async function(storeName) {
        await this.ensureDb();
        return new Promise((resolve, reject) => {
            try {
                const transaction = this.db.transaction([storeName], 'readonly');
                const store = transaction.objectStore(storeName);
                const request = store.count();
                
                request.onsuccess = () => resolve(request.result);
                request.onerror = () => {
                    console.error(`${pomodoroConstants.messages.indexedDbErrorGettingCount} ${storeName}:`, request.error);
                    reject(request.error);
                };
            } catch (error) {
                console.error(pomodoroConstants.messages.indexedDbExceptionGetCount, error);
                reject(error);
            }
        });
    }
};
