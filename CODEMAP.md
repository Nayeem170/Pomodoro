# Code Execution Map

Four-layer Blazor WASM architecture: UI pages depend on services, services depend on repositories, repositories depend on JS interop. All timer events flow through a single `ITimerEventPublisher` interface.

## Architecture Overview

```mermaid
graph TB
    subgraph UI["UI Layer — Razor Pages & Components"]
        direction LR
        Index["📄 Index"]
        History["📊 History"]
        Settings["⚙️ Settings"]
        About["ℹ️ About"]
        TimerDisplay["⏲️ TimerDisplay"]
        TaskList["✅ TaskList"]
        Charts["📈 Charts"]
    end

    subgraph Services["Service Layer — Business Logic"]
        direction LR
        Timer["⏱️ Timer"]
        Task["✅ Tasks"]
        Activity["📋 Activity"]
        Consent["🔔 Consent"]
        Pip["🖼️ PiP"]
        Stats["📊 Stats"]
    end

    subgraph Repos["Repository Layer — Data Access"]
        direction LR
        TaskRepo["TaskRepo"]
        ActRepo["ActivityRepo"]
        SettingsRepo["SettingsRepo"]
        IDB["IndexedDb"]
    end

    subgraph JS["JS Interop Layer — Browser API"]
        direction LR
        TimerJS["timer"]
        IDBJS["indexedDb"]
        PipJS["pip"]
        NotifJS["notify"]
        ChartJS["chart"]
    end

    UI --> Services
    Services --> Repos
    Repos --> IDB
    Services --> JS
```

---

## Service Dependency Graph

Split into 4 diagrams by layer. Pages inject services; services inject repositories or other services; repositories go through IndexedDbService to JS.

### Pages → Services

```mermaid
graph TD
    subgraph Pages["Pages"]
        Index["📄 Index.razor"]
        History["📄 History.razor"]
        Settings["📄 Settings.razor"]
    end

    subgraph Services["Services"]
        TimerService["⏱️ TimerService"]
        TaskService["✅ TaskService"]
        ActivityService["📋 ActivityService"]
        ConsentService["🔔 ConsentService"]
        NotificationService["🔔 NotificationService"]
        PipTimerService["🖼️ PipTimerService"]
        ExportService["📤 ExportService"]
        ImportService["📥 ImportService"]
        StatisticsService["📈 StatisticsService"]
        TodayStatsService["📅 TodayStatsService"]
        KeyboardShortcutService["⌨️ KeyboardShortcut"]
        InfiniteScroll["📜 InfiniteScroll"]
        LocalDateTimeService["🕐 LocalDateTime"]
        HistoryStatsService["HistoryStats"]
    end

    Index --> TimerService
    Index --> TaskService
    Index --> ConsentService
    Index --> NotificationService
    Index --> ActivityService
    Index --> PipTimerService
    Index --> KeyboardShortcutService
    Index --> TodayStatsService
    Index --> InfiniteScroll
    Index --> LocalDateTimeService

    History --> ActivityService
    History --> StatisticsService
    History --> InfiniteScroll
    History --> HistoryStatsService
    History --> LocalDateTimeService

    Settings --> TimerService
    Settings --> ExportService
    Settings --> ImportService
    Settings --> TaskService
    Settings --> ActivityService
```

### Core Services → Their Dependencies

```mermaid
graph TD
    subgraph Core["Core Services"]
        TimerService["⏱️ TimerService"]
        TaskService["✅ TaskService"]
        ActivityService["📋 ActivityService"]
        PipTimerService["🖼️ PipTimerService"]
        ConsentService["🔔 ConsentService"]
    end

    subgraph Extracted["Extracted Services"]
        DailyStatsService["📊 DailyStatsService"]
        JsTimerInterop["🔄 JsTimerInterop"]
        SessionOptionsService["SessionOptions"]
    end

    subgraph Repos["Repositories"]
        TaskRepository["TaskRepository"]
        ActivityRepository["ActivityRepository"]
        SettingsRepository["SettingsRepository"]
        IndexedDbService["🗄️ IndexedDbService"]
    end

    subgraph State["Shared State"]
        AppState["📦 AppState"]
    end

    TimerService --> IndexedDbService
    TimerService --> SettingsRepository
    TimerService --> DailyStatsService
    TimerService --> JsTimerInterop
    TimerService --> AppState

    TaskService --> TaskRepository
    TaskService --> IndexedDbService
    TaskService --> AppState

    ActivityService --> ActivityRepository

    PipTimerService --> TimerService
    PipTimerService --> TaskService
    PipTimerService --> AppState

    ConsentService --> TimerService
    ConsentService --> TaskService
    ConsentService --> NotificationService
    ConsentService --> AppState
    ConsentService --> SessionOptionsService

    DailyStatsService --> IndexedDbService
    DailyStatsService --> AppState

    TaskRepository --> IndexedDbService
    ActivityRepository --> IndexedDbService
    SettingsRepository --> IndexedDbService
```

### Import / Export Services

```mermaid
graph LR
    subgraph Data["Data Services"]
        ImportService["📥 ImportService"]
        ExportService["📤 ExportService"]
    end

    subgraph Repos["Repositories"]
        ActivityRepo["ActivityRepository"]
        TaskRepo["TaskRepository"]
        SettingsRepo["SettingsRepository"]
    end

    ImportService --> ActivityRepo
    ImportService --> TaskRepo
    ImportService --> SettingsRepo

    ExportService --> ActivityRepo
    ExportService --> TaskRepo
    ExportService --> SettingsRepo
```

### UI Interop → JS Runtime

```mermaid
graph LR
    subgraph Services["Services"]
        JsTimerInterop["🔄 JsTimerInterop"]
        PipTimerService["🖼️ PipTimer"]
        NotificationService["🔔 Notification"]
        ChartService["📊 ChartService"]
        KeyboardShortcut["⌨️ Keyboard"]
        LocalDateTime["🕐 LocalDateTime"]
        InfiniteScroll["📜 InfiniteScroll"]
        IndexedDbService["🗄️ IndexedDb"]
    end

    subgraph JS["🌐 IJSRuntime"]
        JSRuntime["IJSRuntime"]
    end

    JsTimerInterop --> JSRuntime
    PipTimerService --> JSRuntime
    NotificationService --> JSRuntime
    ChartService --> JSRuntime
    KeyboardShortcut --> JSRuntime
    LocalDateTime --> JSRuntime
    InfiniteScroll --> JSRuntime
    IndexedDbService --> JSRuntime
```

---

## Event Flow Diagram

All timer events use `ITimerEventPublisher`. `OnTimerCompleted` is async with rich args; `OnTick` and `OnTimerStateChanged` are sync. Wired once at startup by `EventWiringService`.

```mermaid
graph LR
    subgraph Publisher["Publisher"]
        TimerService["⏱️ TimerService"]
    end

    subgraph Subscribers["Subscribers (ITimerEventPublisher)"]
        TaskService["✅ TaskService"]
        ActivityService["📋 ActivityService"]
        ConsentService["🔔 ConsentService"]
        PipTimerService["🖼️ PipTimerService"]
        Index["📄 Index.razor"]
        TimerDisplay["⏲️ TimerDisplay"]
    end

    TimerService -->|"OnTimerCompleted"| TaskService
    TimerService -->|"OnTimerCompleted"| ActivityService
    TimerService -->|"OnTimerCompleted"| ConsentService
    TimerService -->|"OnTimerCompleted"| Index
    TimerService -->|"OnTick"| PipTimerService
    TimerService -->|"OnTick"| TimerDisplay
    TimerService -->|"OnTimerStateChanged"| PipTimerService
    TimerService -->|"OnTimerStateChanged"| Index
    TimerService -->|"OnTimerStateChanged"| TimerDisplay
```

---

## Sequence: Timer Start

User clicks start, session is created, JS timer begins ticking, and `OnTick`/`OnTimerStateChanged` events drive UI updates every second.

```mermaid
sequenceDiagram
    actor U as 👤 User
    participant I as 📄 Index.razor
    participant TS as ⏱️ TimerService
    participant JTI as 🔄 JsTimerInterop
    participant JS as 🌐 JS Runtime
    participant AS as 📦 AppState

    U->>I: Click Start
    I->>TS: StartPomodoroAsync(taskId)
    TS->>AS: Set CurrentSession (Pomodoro, running)
    TS->>AS: Set SelectedTaskId
    TS->>TS: NotifyStateChanged()
    TS--xI: OnStateChanged event
    I->>I: StateHasChanged()
    TS->>JTI: StartAsync(dotNetRef)
    JTI->>JS: notificationFunctions.unlockAudio()
    JTI->>JS: timerFunctions.start(dotNetRef)
    JS--xU: ⏱️ Timer ticking (1s)
    JS->>TS: OnTimerTickJs() [JSInvokable]
    TS->>AS: Decrement RemainingSeconds
    TS--xI: OnTick event
    I->>I: StateHasChanged()
```

---

## Sequence: Timer Complete

When remaining seconds hit zero: stats are recorded, subscribers are notified in sequence (task completion, activity recording, consent modal), and PiP window is updated.

```mermaid
sequenceDiagram
    participant JS as 🌐 JS Runtime
    participant TS as ⏱️ TimerService
    participant DSS as 📊 DailyStatsService
    participant TaskSvc as ✅ TaskService
    participant ActSvc as 📋 ActivityService
    participant Consent as 🔔 ConsentService
    participant Pip as 🖼️ PipTimerService
    participant I as 📄 Index.razor
    participant AS as 📦 AppState

    JS->>TS: OnTimerTickJs() (remaining=0)
    TS->>TS: HandleTimerCompleteAsync()
    TS->>JTI: StopAsync()

    Note over TS,DSS: Update Daily Stats
    TS->>DSS: RecordPomodoroCompletion(min, taskId)
    DSS->>AS: ++TodayPomodoroCount, +=FocusMinutes
    TS->>TS: SaveDailyStatsAsync() → IndexedDB

    Note over TS,Consent: Notify Subscribers
    TS->>TS: NotifyTimerCompletedAsync(args)
    TS--xTaskSvc: OnTimerCompleted
    TaskSvc->>TaskSvc: CompleteTaskAsync()
    TS--xActSvc: OnTimerCompleted
    ActSvc->>ActSvc: RecordActivityAsync()
    TS--xConsent: OnTimerCompleted
    Consent->>Consent: ShowConsentModal()
    TS--xI: OnTimerCompleted
    I->>I: Update state, show consent UI

    TS->>TS: NotifyStateChanged()
    TS--xPip: OnTimerStateChanged
    Pip->>JS: pipTimer.update(state)
```

---

## Sequence: App Initialization

On startup: IndexedDB is initialized, services load cached data, and `EventWiringService` connects publisher to subscribers before the first render.

```mermaid
sequenceDiagram
    participant App as 🚀 App.razor
    participant SIS as ⚙️ ServiceInitializationService
    participant IDB as 🗄️ IndexedDbService
    participant TS as ⏱️ TimerService
    participant DSS as 📊 DailyStatsService
    participant TaskSvc as ✅ TaskService
    participant ActSvc as 📋 ActivityService
    participant EWS as 🔗 EventWiringService
    participant I as 📄 Index.razor

    App->>SIS: InitializeServicesAsync()

    Note over SIS,IDB: Database Init
    SIS->>IDB: InitializeAsync()
    IDB->>JS: indexedDbInterop.initDatabase()

    Note over SIS,ActSvc: Service Init
    SIS->>TS: InitializeAsync()
    TS->>SettingsRepo: GetAsync() → TimerSettings
    TS->>DSS: InitializeTodayStatsAsync()
    DSS->>IDB: GetAsync<DailyStats>()
    DSS->>AppState: Restore daily counters
    SIS->>TaskSvc: InitializeAsync()
    TaskSvc->>IDB: GetAllItems<TaskItem>()
    SIS->>ActSvc: InitializeAsync()
    ActSvc->>IDB: GetAllItems<ActivityRecord>()

    Note over EWS: Event Wiring
    SIS->>EWS: WireEventSubscribers()
    EWS->>TS: OnTimerCompleted += TaskService
    EWS->>TS: OnTimerCompleted += ActivityService
    EWS->>TS: OnTimerCompleted += ConsentService
    EWS->>TS: OnTick += PipTimerService
    EWS->>TS: OnTimerStateChanged += PipTimerService

    App->>I: Render page
    I->>TS: OnStateChanged += handler
    I->>TS: OnTimerComplete += handler
    I->>TaskSvc: OnTasksChanged += handler
```

---

## JS Interop Map

47 interop points total: 41 .NET-to-JS calls across 9 modules, plus 9 JS-to-.NET callbacks via `[JSInvokable]`.

### .NET → JS Calls

```mermaid
graph LR
    subgraph NET[".NET Services"]
        IDB["🗄️ IndexedDbService"]
        JTI["🔄 JsTimerInterop"]
        Pip["🖼️ PipTimerService"]
        Notif["🔔 NotificationService"]
        Chart["📊 ChartService"]
        KBD["⌨️ KeyboardShortcut"]
        LDT["🕐 LocalDateTime"]
        IS["📜 InfiniteScroll"]
        JSI["JSInteropService"]
    end

    subgraph JSModules["JS Modules"]
        IDBJS["indexedDbInterop<br/><i>11 functions</i>"]
        TimerJS["timerFunctions<br/><i>2 functions</i>"]
        PipJS["pipTimer<br/><i>6 functions</i>"]
        NotifJS["notificationFunctions<br/><i>7 functions</i>"]
        ChartJS["chartInterop<br/><i>6 functions</i>"]
        KBDJS["keyboardShortcuts<br/><i>2 functions</i>"]
        LDTJS["localDateTime<br/><i>3 functions</i>"]
        ISJS["infiniteScroll<br/><i>4 functions</i>"]
        GlobalJS["Global<br/><i>2 functions</i>"]
    end

    IDB --> IDBJS
    JTI --> TimerJS
    JTI --> NotifJS
    Pip --> PipJS
    Notif --> NotifJS
    Chart --> ChartJS
    KBD --> KBDJS
    LDT --> LDTJS
    IS --> ISJS
    JSI --> GlobalJS
```

| Module | Functions | Called By |
|--------|-----------|-----------|
| `indexedDbInterop` | `initDatabase`, `getItem`, `getAllItems`, `getItemsByIndex`, `getItemsByDateRange`, `putItem`, `putAllItems`, `deleteItem`, `clearStore`, `getCount`, `initializeJsConstants` | IndexedDbService |
| `timerFunctions` | `start`, `stop` | JsTimerInterop |
| `pipTimer` | `isSupported`, `registerDotNetRef`, `unregisterDotNetRef`, `open`, `close`, `update` | PipTimerService |
| `notificationFunctions` | `registerDotNetRef`, `unregisterDotNetRef`, `requestNotificationPermission`, `showNotification`, `playTimerCompleteSound`, `playBreakCompleteSound`, `unlockAudio` | NotificationService, JsTimerInterop |
| `chartInterop` | `createBarChart`, `createGroupedBarChart`, `createDoughnutChart`, `updateChart`, `destroyChart`, `ensureInitialized` | ChartService |
| `keyboardShortcuts` | `initialize`, `dispose` | KeyboardShortcutService |
| `localDateTime` | `getLocalDate`, `getLocalDateTime`, `getTimezoneOffset` | LocalDateTimeService |
| `infiniteScroll` | `isSupported`, `createObserver`, `destroyObserver`, `destroyAllObservers` | InfiniteScrollInterop |
| Global | `getUrlParameter`, `removeUrlParameter` | JSInteropService |

### JS → .NET Callbacks

9 `[JSInvokable]` methods across 6 classes. Timer tick is the highest-frequency callback (every second).

| Method | Class | Trigger |
|--------|-------|---------|
| `OnTimerTickJs` | TimerService | Timer tick (1s interval) |
| `OnPipToggleTimer` | PipTimerService | PiP play/pause button |
| `OnPipResetTimer` | PipTimerService | PiP reset button |
| `OnPipSwitchSession` | PipTimerService | PiP session tab |
| `OnPipClosed` | PipTimerService | PiP window close |
| `OnNotificationActionClick` | NotificationService | Browser notification click |
| `OnSentinelIntersecting` | HistoryBase | Scroll sentinel visible |
| `HandleShortcut` | KeyboardShortcutService | Key press |
| `NavigateTo` | MainLayout | Navigation event |

---

## Data Flow: IndexedDB

4 stores: tasks and activities are read/write from multiple services; settings and daily stats have single-writer patterns.

```mermaid
graph LR
    subgraph Stores["IndexedDB Stores"]
        Tasks["tasks<br/>TaskItem[]"]
        Activities["activities<br/>ActivityRecord[]"]
        Settings["settings<br/>TimerSettings"]
        DailyStats["dailyStats<br/>DailyStats"]
    end

    subgraph Writers["Writers"]
        TaskSvc["TaskService"]
        ActSvc["ActivityService"]
        TimerSvc["TimerService"]
        ImportSvc["ImportService"]
        SettingsRepo["SettingsRepository"]
    end

    subgraph Readers["Readers"]
        ExportSvc["ExportService"]
        DailyStatsSvc["DailyStatsService"]
        StatisticsSvc["StatisticsService"]
    end

    TaskSvc -->|"CRUD"| Tasks
    ActSvc -->|"CRUD"| Activities
    TimerSvc -->|"Put"| DailyStats
    ImportSvc -->|"Write"| Tasks
    ImportSvc -->|"Write"| Activities
    ImportSvc -->|"Write"| Settings
    SettingsRepo -->|"Get/Save"| Settings

    ExportSvc -->|"Read"| Tasks
    ExportSvc -->|"Read"| Activities
    DailyStatsSvc -->|"Get"| DailyStats
    StatisticsSvc -->|"Read"| Activities
```

---

## Page Injection Summary

### Index.razor (12 services)

The main page with the most dependencies. It orchestrates timer, tasks, consent, notifications, PiP, keyboard shortcuts, and today's stats display.

| Service | Purpose |
|---------|---------|
| `ITaskService` | Task CRUD, selection, completion |
| `ITimerService` | Timer start/pause/reset |
| `IConsentService` | Post-pomodoro consent modal |
| `INotificationService` | Browser notifications |
| `IActivityService` | Activity history queries |
| `IPipTimerService` | Picture-in-Picture timer |
| `AppState` | Global state (session, settings, stats) |
| `IJSRuntime` | Direct JS interop |
| `IKeyboardShortcutService` | Keyboard shortcuts |
| `ITodayStatsService` | Today's summary stats |
| `IInfiniteScrollInterop` | Infinite scroll for tasks |
| `ILocalDateTimeService` | Local date/time |
| `ILogger<IndexBase>` | Logging |

### History.razor (8 services)

Activity history with infinite scroll, weekly charts, and time distribution. Reads only — no writes to data stores.

| Service | Purpose |
|---------|---------|
| `IActivityService` | Activity queries |
| `IStatisticsService` | Weekly stats, time distribution |
| `IJSRuntime` | Direct JS interop |
| `IInfiniteScrollInterop` | Infinite scroll for activities |
| `IHistoryStatsService` | Daily summary formatting |
| `HistoryPagePresenterService` | View formatting logic |
| `ILocalDateTimeService` | Local date/time |
| `ILogger<HistoryBase>` | Logging |

### Settings.razor (8 services)

App configuration page with export/import, timer settings, and data management. Uses both Export and Import services for backup/restore.

| Service | Purpose |
|---------|---------|
| `ITimerService` | Timer settings reference |
| `IExportService` | Data export (JSON) |
| `IImportService` | Data import (JSON) |
| `ITaskService` | Task data for import/export |
| `IActivityService` | Activity data for import/export |
| `IJSInteropService` | JS interop (file picker) |
| `SettingsPresenterService` | Settings formatting logic |
| `ILogger<SettingsPageBase>` | Logging |
