# Code Execution Map

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    Blazor WASM App                       │
│                                                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────┐ │
│  │  Index    │  │ History  │  │ Settings │  │ About  │ │
│  │  .razor   │  │  .razor  │  │  .razor  │  │ .razor │ │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────────┘ │
│       │              │              │                    │
│  ┌────▼──────────────▼──────────────▼─────────────────┐ │
│  │              Service Layer                          │ │
│  │  TimerService · TaskService · ActivityService       │ │
│  │  PipTimerService · ConsentService · ChartService    │ │
│  └────────────────────┬───────────────────────────────┘ │
│                       │                                  │
│  ┌────────────────────▼───────────────────────────────┐ │
│  │           Repository Layer                          │ │
│  │  ActivityRepository · TaskRepository                │ │
│  │  SettingsRepository · IndexedDbService              │ │
│  └────────────────────┬───────────────────────────────┘ │
│                       │                                  │
│  ┌────────────────────▼───────────────────────────────┐ │
│  │           JS Interop Layer                          │ │
│  │  timerFunctions · indexedDbInterop · pipTimer       │ │
│  │  notificationFunctions · chartInterop · localDateTime│ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

---

## Service Dependency Graph

```mermaid
graph TD
    subgraph Pages
        Index["Index.razor"]
        History["History.razor"]
        Settings["Settings.razor"]
    end

    subgraph Services
        TimerService["TimerService"]
        TaskService["TaskService"]
        ActivityService["ActivityService"]
        PipTimerService["PipTimerService"]
        ConsentService["ConsentService"]
        DailyStatsService["DailyStatsService"]
        JsTimerInterop["JsTimerInterop"]
        StatisticsService["StatisticsService"]
        ImportService["ImportService"]
        ExportService["ExportService"]
        NotificationService["NotificationService"]
        ChartService["ChartService"]
        KeyboardShortcutService["KeyboardShortcutService"]
        LocalDateTimeService["LocalDateTimeService"]
        HistoryStatsService["HistoryStatsService"]
        SessionOptionsService["SessionOptionsService"]
        TodayStatsService["TodayStatsService"]
    end

    subgraph Repositories
        IndexedDbService["IndexedDbService"]
        ActivityRepository["ActivityRepository"]
        TaskRepository["TaskRepository"]
        SettingsRepository["SettingsRepository"]
    end

    subgraph State
        AppState["AppState"]
    end

    subgraph JS
        JSRuntime["IJSRuntime"]
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

    History --> ActivityService
    History --> StatisticsService
    History --> JSRuntime
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
    subgraph Publisher
        TimerService["TimerService<br/>(ITimerEventPublisher)"]
    end

    subgraph "Timer Completed Subscribers"
        TaskService["TaskService<br/>HandleTimerCompletedAsync"]
        ActivityService["ActivityService<br/>HandleTimerCompletedAsync"]
        ConsentService["ConsentService<br/>HandleTimerCompletedAsync"]
    end

    subgraph "Tick/State Subscribers"
        PipTimerService["PipTimerService<br/>HandleTimerTick<br/>HandleTimerStateChanged"]
    end

    subgraph "UI Subscribers (old events)"
        Index["Index.razor<br/>OnTimerComplete<br/>OnStateChanged"]
        TimerDisplay["TimerDisplay<br/>OnTick<br/>OnStateChanged"]
    end

    TimerService -->|"OnTimerCompleted"| TaskService
    TimerService -->|"OnTimerCompleted"| ActivityService
    TimerService -->|"OnTimerCompleted"| ConsentService
    TimerService -->|"OnTick"| PipTimerService
    TimerService -->|"OnTimerStateChanged"| PipTimerService
    TimerService -.->|"OnTimerComplete (legacy)"| Index
    TimerService -.->|"OnStateChanged (legacy)"| Index
    TimerService -.->|"OnTick (legacy)"| TimerDisplay
    TimerService -.->|"OnStateChanged (legacy)"| TimerDisplay
```

---

## Sequence: Timer Start (Pomodoro)

```mermaid
sequenceDiagram
    actor User
    participant Index as Index.razor
    participant TS as TimerService
    participant JTI as JsTimerInterop
    participant JS as JS Runtime
    participant AS as AppState

    User->>Index: Click Start
    Index->>TS: StartPomodoroAsync(taskId)
    TS->>AS: Set CurrentSession (type=Pomodoro, running=true)
    TS->>AS: Set SelectedTaskId
    TS->>TS: NotifyStateChanged()
    TS-->>Index: OnStateChanged event
    Index->>Index: StateHasChanged()
    TS->>JTI: StartAsync(dotNetRef)
    JTI->>JS: unlockAudio()
    JTI->>JS: timerFunctions.start(dotNetRef)
    JS-->>User: Timer ticking (1s interval)
    JS->>TS: OnTimerTickJs() [JSInvokable]
    TS->>AS: Decrement RemainingSeconds
    TS-->>Index: OnTick event
    Index->>Index: StateHasChanged()
```

---

## Sequence: Timer Complete

```mermaid
sequenceDiagram
    participant JS as JS Runtime
    participant TS as TimerService
    participant DSS as DailyStatsService
    participant TaskSvc as TaskService
    participant ActSvc as ActivityService
    participant Consent as ConsentService
    participant Pip as PipTimerService
    participant Index as Index.razor
    participant AS as AppState

    JS->>TS: OnTimerTickJs() (remaining = 0)
    TS->>TS: HandleTimerCompleteAsync()
    TS->>JTI: StopAsync()
    TS->>DSS: RecordPomodoroCompletion(minutes, taskId)
    DSS->>AS: Increment TodayPomodoroCount
    DSS->>AS: Add TodayTotalFocusMinutes
    TS->>TS: SaveDailyStatsAsync() → IndexedDB
    TS->>TS: NotifyTimerCompletedAsync(args)
    TS-->>TaskSvc: OnTimerCompleted → HandleTimerCompletedAsync()
    TaskSvc->>TaskSvc: CompleteTaskAsync()
    TS-->>ActSvc: OnTimerCompleted → HandleTimerCompletedAsync()
    ActSvc->>ActSvc: RecordActivityAsync()
    TS-->>Consent: OnTimerCompleted → HandleTimerCompletedAsync()
    Consent->>Consent: ShowConsentModal()
    TS->>TS: OnTimerComplete (legacy event)
    TS-->>Index: OnTimerComplete event
    Index->>Index: Show consent UI
    TS->>TS: NotifyStateChanged()
    TS-->>Pip: OnTimerStateChanged → HandleTimerStateChanged()
    Pip->>Pip: UpdateTimerAsync() → PiP window
```

---

## Sequence: Page Initialization

```mermaid
sequenceDiagram
    participant App as App.razor
    participant SIS as ServiceInitializationService
    participant IDB as IndexedDbService
    participant TS as TimerService
    participant TaskSvc as TaskService
    participant ActSvc as ActivityService
    participant EWS as EventWiringService
    participant Index as Index.razor

    App->>SIS: InitializeServicesAsync()
    SIS->>IDB: InitializeAsync()
    IDB->>JS: indexedDbInterop.initDatabase()
    SIS->>TS: InitializeAsync()
    TS->>SR: GetAsync() → load settings
    TS->>DSS: InitializeTodayStatsAsync()
    DSS->>IDB: GetAsync<DailyStats>()
    DSS->>AS: Restore TodayTotalFocusMinutes, PomodoroCount
    SIS->>TaskSvc: InitializeAsync()
    TaskSvc->>IDB: GetAllItems<TaskItem>()
    SIS->>ActSvc: InitializeAsync()
    ActSvc->>IDB: GetAllItems<ActivityRecord>()
    SIS->>EWS: WireEventSubscribers()
    EWS->>TS: OnTimerCompleted += TaskService
    EWS->>TS: OnTimerCompleted += ActivityService
    EWS->>TS: OnTimerCompleted += ConsentService
    EWS->>TS: OnTick += PipTimerService
    EWS->>TS: OnTimerStateChanged += PipTimerService
    App->>Index: OnInitializedAsync()
    Index->>TS: OnStateChanged += handler
    Index->>TS: OnTimerComplete += handler
    Index->>TaskSvc: OnTasksChanged += handler
```

---

## JS Interop Map

### .NET → JS Calls (41 functions)

| Module | Functions | Called By |
|--------|-----------|-----------|
| `timerFunctions` | `start`, `stop` | JsTimerInterop |
| `indexedDbInterop` | `initDatabase`, `getItem`, `getAllItems`, `getItemsByIndex`, `getItemsByDateRange`, `putItem`, `putAllItems`, `deleteItem`, `clearStore`, `getCount` | IndexedDbService |
| `pomodoroConstants` | `initialize` | IndexedDbService |
| `pipTimer` | `isSupported`, `registerDotNetRef`, `unregisterDotNetRef`, `open`, `close`, `update` | PipTimerService |
| `notificationFunctions` | `registerDotNetRef`, `unregisterDotNetRef`, `requestNotificationPermission`, `showNotification`, `playTimerCompleteSound`, `playBreakCompleteSound`, `unlockAudio` | NotificationService, JsTimerInterop |
| `localDateTime` | `getLocalDate`, `getLocalDateTime`, `getTimezoneOffset` | LocalDateTimeService |
| `keyboardShortcuts` | `initialize`, `dispose` | KeyboardShortcutService |
| `infiniteScroll` | `isSupported`, `createObserver`, `destroyObserver`, `destroyAllObservers` | InfiniteScrollInterop |
| `chartInterop` | `createBarChart`, `createGroupedBarChart`, `createDoughnutChart`, `updateChart`, `destroyChart`, `ensureInitialized` | ChartService |
| Global | `getUrlParameter`, `removeUrlParameter` | JSInteropService |

### JS → .NET Callbacks (6 [JSInvokable] methods)

| Method | On Class | Triggered By |
|--------|----------|-------------|
| `OnTimerTickJs` | TimerService | `timerFunctions.start` (1s interval) |
| `OnPipToggleTimer` | PipTimerService | PiP window play/pause button |
| `OnPipResetTimer` | PipTimerService | PiP window reset button |
| `OnPipSwitchSession` | PipTimerService | PiP window session tab |
| `OnPipClosed` | PipTimerService | PiP window close |
| `OnNotificationActionClick` | NotificationService | Browser notification click |

---

## Dual Event System (Technical Debt)

TimerService currently fires **both** old and new events for backward compatibility:

| Notification | Old Event (ITimerService) | New Event (ITimerEventPublisher) |
|---|---|---|
| Tick | `OnTick` | `OnTick` (same name, same backing field) |
| State changed | `OnStateChanged` | `OnTimerStateChanged` |
| Timer completed | `OnTimerComplete(SessionType)` | `OnTimerCompleted(TimerCompletedEventArgs)` |

**Subscribers by event system:**

| Subscriber | Uses Old Events | Uses New Events |
|---|---|---|
| Index.razor | `OnTimerComplete`, `OnStateChanged` | - |
| TimerDisplay.razor | `OnTick`, `OnStateChanged` | - |
| TaskService | - | `OnTimerCompleted` |
| ActivityService | - | `OnTimerCompleted` |
| ConsentService | - | `OnTimerCompleted` |
| PipTimerService | - | `OnTick`, `OnTimerStateChanged` |

---

## Data Flow: IndexedDB Stores

```mermaid
graph LR
    subgraph Stores
        Tasks["tasks store<br/>TaskItem[]"]
        Activities["activities store<br/>ActivityRecord[]"]
        Settings["settings store<br/>TimerSettings"]
        DailyStats["dailyStats store<br/>DailyStats"]
    end

    TaskService -->|"CRUD"| Tasks
    ActivityService -->|"CRUD"| Activities
    SettingsRepository -->|"Get/Save"| Settings
    TimerService -->|"Put (daily)"| DailyStats
    DailyStatsService -->|"Get (init)"| DailyStats
    ExportService -->|"Read all"| Tasks
    ExportService -->|"Read all"| Activities
    ImportService -->|"Write all"| Tasks
    ImportService -->|"Write all"| Activities
    ImportService -->|"Write"| Settings
```
