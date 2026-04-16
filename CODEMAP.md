# Code Execution Map

## Architecture Overview

```mermaid
graph TB
    subgraph UI["UI Layer"]
        direction LR
        Index["🏠 Index"]
        History["📊 History"]
        Settings["⚙️ Settings"]
        About["ℹ️ About"]
    end

    subgraph Services["Service Layer"]
        direction LR
        Timer["TimerService"]
        Task["TaskService"]
        Activity["ActivityService"]
        Pip["PipTimerService"]
        Consent["ConsentService"]
        Chart["ChartService"]
    end

    subgraph Repos["Repository Layer"]
        direction LR
        ActRepo["ActivityRepository"]
        TaskRepo["TaskRepository"]
        SettingsRepo["SettingsRepository"]
        IDB["IndexedDbService"]
    end

    subgraph JS["JS Interop Layer"]
        direction LR
        TimerJS["timerFunctions"]
        IDBJS["indexedDbInterop"]
        PipJS["pipTimer"]
        NotifJS["notificationFunctions"]
        ChartJS["chartInterop"]
    end

    UI --> Services
    Services --> Repos
    Repos --> IDB
    Services --> JS
```

---

## Service Dependency Graph

```mermaid
graph TD
    subgraph Pages["Pages"]
        Index["📄 Index.razor<br/><i>12 injected services</i>"]
        History["📄 History.razor<br/><i>8 injected services</i>"]
        Settings["📄 Settings.razor<br/><i>8 injected services</i>"]
    end

    subgraph CoreServices["Core Services"]
        TimerService["⏱️ TimerService<br/><i>ITimerService, ITimerEventPublisher</i>"]
        TaskService["✅ TaskService<br/><i>ITaskService, ITimerEventSubscriber</i>"]
        ActivityService["📋 ActivityService<br/><i>IActivityService, ITimerEventSubscriber</i>"]
        PipTimerService["🖼️ PipTimerService<br/><i>IPipTimerService, ITimerEventPublisherSubscriber</i>"]
        ConsentService["🔔 ConsentService<br/><i>IConsentService, ITimerEventSubscriber</i>"]
    end

    subgraph ExtractedServices["Extracted Services"]
        DailyStatsService["📊 DailyStatsService"]
        JsTimerInterop["🔄 JsTimerInterop"]
        StatisticsService["📈 StatisticsService"]
        ImportService["📥 ImportService"]
        ExportService["📤 ExportService"]
        TodayStatsService["📅 TodayStatsService"]
    end

    subgraph UIInterop["UI Interop Services"]
        NotificationService["🔔 NotificationService"]
        ChartService["📊 ChartService"]
        KeyboardShortcutService["⌨️ KeyboardShortcutService"]
        LocalDateTimeService["🕐 LocalDateTimeService"]
        InfiniteScrollInterop["📜 InfiniteScrollInterop"]
    end

    subgraph Repositories["Repositories"]
        IndexedDbService["🗄️ IndexedDbService"]
        ActivityRepository["ActivityRepository"]
        TaskRepository["TaskRepository"]
        SettingsRepository["SettingsRepository"]
    end

    subgraph State["State"]
        AppState["📦 AppState<br/><i>Singleton</i>"]
    end

    subgraph Infra["Infrastructure"]
        JSRuntime["🌐 IJSRuntime"]
        SessionOptionsService["SessionOptionsService"]
        HistoryStatsService["HistoryStatsService"]
    end

    Index --> TimerService
    Index --> TaskService
    Index --> ConsentService
    Index --> NotificationService
    Index --> ActivityService
    Index --> PipTimerService
    Index --> AppState
    Index --> JSRuntime
    Index --> KeyboardShortcutService
    Index --> TodayStatsService
    Index --> InfiniteScrollInterop
    Index --> LocalDateTimeService

    History --> ActivityService
    History --> StatisticsService
    History --> JSRuntime
    History --> InfiniteScrollInterop
    History --> HistoryStatsService
    History --> LocalDateTimeService

    Settings --> TimerService
    Settings --> ExportService
    Settings --> ImportService
    Settings --> TaskService
    Settings --> ActivityService

    TimerService --> IndexedDbService
    TimerService --> SettingsRepository
    TimerService --> DailyStatsService
    TimerService --> JsTimerInterop
    TimerService --> AppState

    DailyStatsService --> IndexedDbService
    DailyStatsService --> AppState

    JsTimerInterop --> JSRuntime

    TaskService --> TaskRepository
    TaskService --> IndexedDbService
    TaskService --> AppState

    ActivityService --> ActivityRepository

    StatisticsService --> ActivityRepository

    ImportService --> ActivityRepository
    ImportService --> TaskRepository
    ImportService --> SettingsRepository

    ExportService --> ActivityRepository
    ExportService --> TaskRepository
    ExportService --> SettingsRepository

    PipTimerService --> JSRuntime
    PipTimerService --> TimerService
    PipTimerService --> TaskService
    PipTimerService --> AppState

    ConsentService --> TimerService
    ConsentService --> TaskService
    ConsentService --> NotificationService
    ConsentService --> AppState
    ConsentService --> SessionOptionsService

    NotificationService --> JSRuntime
    NotificationService --> AppState

    ChartService --> JSRuntime

    KeyboardShortcutService --> JSRuntime

    LocalDateTimeService --> JSRuntime

    SessionOptionsService --> AppState

    TodayStatsService --> ActivityService

    ActivityRepository --> IndexedDbService
    TaskRepository --> IndexedDbService
    SettingsRepository --> IndexedDbService

    IndexedDbService --> JSRuntime
```

---

## Event Flow Diagram

```mermaid
graph LR
    subgraph Publisher["Publisher"]
        TimerService["⏱️ TimerService"]
    end

    subgraph NewSubscribers["New Event System (ITimerEventPublisher)"]
        TaskService["✅ TaskService"]
        ActivityService["📋 ActivityService"]
        ConsentService["🔔 ConsentService"]
        PipTimerService["🖼️ PipTimerService"]
    end

    subgraph OldSubscribers["Legacy Events (ITimerService)"]
        Index["📄 Index.razor"]
        TimerDisplay["⏲️ TimerDisplay"]
    end

    TimerService -->|"OnTimerCompleted"| TaskService
    TimerService -->|"OnTimerCompleted"| ActivityService
    TimerService -->|"OnTimerCompleted"| ConsentService
    TimerService -->|"OnTick"| PipTimerService
    TimerService -->|"OnTimerStateChanged"| PipTimerService

    TimerService -.->|"OnTimerComplete (legacy)"| Index
    TimerService -.->|"OnStateChanged (legacy)"| Index
    TimerService -.->|"OnTick (shared)"| TimerDisplay
    TimerService -.->|"OnStateChanged (legacy)"| TimerDisplay
```

---

## Sequence: Timer Start

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

    Note over TS,Consent: Notify Subscribers (ITimerEventPublisher)
    TS->>TS: NotifyTimerCompletedAsync(args)
    TS--xTaskSvc: OnTimerCompleted
    TaskSvc->>TaskSvc: CompleteTaskAsync()
    TS--xActSvc: OnTimerCompleted
    ActSvc->>ActSvc: RecordActivityAsync()
    TS--xConsent: OnTimerCompleted
    Consent->>Consent: ShowConsentModal()

    Note over TS,I: Legacy Events (ITimerService)
    TS->>TS: OnTimerComplete?.Invoke(type)
    TS--xI: OnTimerComplete event
    I->>I: Show consent UI

    TS->>TS: NotifyStateChanged()
    TS--xPip: OnTimerStateChanged
    Pip->>JS: pipTimer.update(state)
```

---

## Sequence: App Initialization

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

## Dual Event System

TimerService fires both old and new events for backward compatibility:

```mermaid
graph TB
    subgraph Events["TimerService Events"]
        direction LR
        Old["Legacy (ITimerService)<br/>OnTick, OnStateChanged,<br/>OnTimerComplete"]
        New["Modern (ITimerEventPublisher)<br/>OnTick, OnTimerStateChanged,<br/>OnTimerCompleted"]
    end

    Old -.->|"Fragile dual-fire"| New
```

| Notification | Legacy Event | Modern Event |
|---|---|---|
| Tick | `OnTick` (Action) | `OnTick` (Action) — same backing field |
| State changed | `OnStateChanged` (Action) | `OnTimerStateChanged` (Action) |
| Timer completed | `OnTimerComplete` (Action\<SessionType\>) | `OnTimerCompleted` (Func\<TimerCompletedEventArgs, Task\>) |

| Subscriber | System | Events |
|---|---|---|
| Index.razor | Legacy | `OnTimerComplete`, `OnStateChanged` |
| TimerDisplay.razor | Legacy | `OnTick`, `OnStateChanged` |
| TaskService | Modern | `OnTimerCompleted` |
| ActivityService | Modern | `OnTimerCompleted` |
| ConsentService | Modern | `OnTimerCompleted` |
| PipTimerService | Modern | `OnTick`, `OnTimerStateChanged` |

---

## Data Flow: IndexedDB

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
