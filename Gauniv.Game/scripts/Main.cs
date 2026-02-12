using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Gauniv.Game;

public partial class Main : Control
{
    private readonly PackedScene _loginScene = GD.Load<PackedScene>("res://scenes/LoginScreen.tscn");
    private readonly PackedScene _roomListScene = GD.Load<PackedScene>("res://scenes/RoomListScreen.tscn");
    private readonly PackedScene _roleSelectScene = GD.Load<PackedScene>("res://scenes/RoleSelectScreen.tscn");
    private readonly PackedScene _lobbyScene = GD.Load<PackedScene>("res://scenes/LobbyScreen.tscn");
    private readonly PackedScene _gameScene = GD.Load<PackedScene>("res://scenes/GameScreen.tscn");
    private readonly PackedScene _resultScene = GD.Load<PackedScene>("res://scenes/ResultScreen.tscn");

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private Label _statusLabel = default!;
    private MarginContainer _screenHost = default!;
    private RichTextLabel _logOutput = default!;
    private NetworkClient _network = default!;

    private LoginScreen? _currentLogin;
    private RoomListScreen? _currentRoomList;
    private RoleSelectScreen? _currentRoleSelect;
    private LobbyScreen? _currentLobby;
    private GameScreen? _currentGame;
    private ResultScreen? _currentResult;

    private bool _isAuthenticated;
    private string _sessionId = string.Empty;
    private string _displayName = string.Empty;
    private string _roomId = string.Empty;
    private string _role = "player";
    private string _phase = "Waiting";
    private int _boardSize = 5;
    private string _mjSessionId = string.Empty;
    private int? _targetRow;
    private int? _targetCol;

    private List<RoomSummaryModel> _rooms = new();
    private List<PlayerStateModel> _players = new();
    private List<ObserverStateModel> _observers = new();
    private RoundResultPayloadModel? _lastResult;
    private string _pendingJoinRoomId = string.Empty;

    public override void _Ready()
    {
        Theme = GlassTheme.Build();

        _statusLabel = GetNode<Label>("Root/Stack/StatusLabel");
        _screenHost = GetNode<MarginContainer>("Root/Stack/ScreenFrame/ScreenHost");
        _logOutput = GetNode<RichTextLabel>("Root/Stack/LogFrame/LogOutput");
        _network = GetNode<NetworkClient>("NetworkClient");

        _network.Connected += OnConnected;
        _network.Disconnected += OnDisconnected;
        _network.TransportError += OnTransportError;
        _network.MessageReceived += OnMessageReceived;

        GetNode<Label>("Root/Stack/TitleLabel").Text = "Gauniv Glass Arena";

        SetStatus("Disconnected");
        try
        {
            ShowLoginScreen();
            Log("App ready.");
        }
        catch (Exception ex)
        {
            SetStatus($"Startup error: {ex.Message}");
            Log($"Startup error: {ex}");
        }
    }

    private void OnConnected()
    {
        SetStatus("Connected to game server.");
        _currentLogin?.SetConnectionState(true);
        Log("Connected.");
    }

    private void OnDisconnected()
    {
        _isAuthenticated = false;
        _sessionId = string.Empty;
        _displayName = string.Empty;
        ResetRoomState();
        SetStatus("Disconnected");
        ShowLoginScreen();
        _currentLogin?.SetConnectionState(false);
        Log("Disconnected.");
    }

    private void OnTransportError(string message)
    {
        Log($"Transport error: {message}");
        SetStatus($"Transport error: {message}");
    }

    private void OnMessageReceived(string type, string payloadJson)
    {
        Log($"<= {type} {payloadJson}");

        switch (type)
        {
            case "server.hello":
                HandleServerHello(payloadJson);
                break;
            case "auth.ok":
                HandleAuthOk(payloadJson);
                break;
            case "room.list":
                HandleRoomList(payloadJson);
                break;
            case "room.created":
                HandleRoomCreated(payloadJson);
                break;
            case "room.joined":
                HandleRoomJoined(payloadJson);
                break;
            case "room.left":
                HandleRoomLeft();
                break;
            case "room.state":
                HandleRoomState(payloadJson);
                break;
            case "round.started":
                HandleRoundStarted(payloadJson);
                break;
            case "round.target":
                HandleRoundTarget(payloadJson);
                break;
            case "round.result":
                HandleRoundResult(payloadJson);
                break;
            case "click.accepted":
                SetStatus("Click accepted.");
                break;
            case "error":
                HandleError(payloadJson);
                break;
        }
    }

    private void HandleServerHello(string payloadJson)
    {
        var payload = DeserializePayload<ServerHelloPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _sessionId = payload.SessionId ?? string.Empty;
        SetStatus($"Connected. Session={_sessionId}");
    }

    private void HandleAuthOk(string payloadJson)
    {
        var payload = DeserializePayload<AuthOkPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _isAuthenticated = true;
        _displayName = payload.DisplayName ?? payload.Email ?? "Player";
        SetStatus($"Authenticated as {_displayName}");

        ShowRoomListScreen();
        _ = SendMessageAsync("room.list", new Dictionary<string, object?>());
    }

    private void HandleRoomList(string payloadJson)
    {
        var payload = DeserializePayload<RoomListPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _rooms = payload.Rooms ?? new List<RoomSummaryModel>();
        _currentRoomList?.SetRooms(_rooms);
    }

    private void HandleRoomCreated(string payloadJson)
    {
        var payload = DeserializePayload<RoomCreatedPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _pendingJoinRoomId = payload.RoomId ?? string.Empty;
        SetStatus($"Room created: {_pendingJoinRoomId}");
        _currentRoomList?.SetSelectedRoom(_pendingJoinRoomId);
        _ = SendMessageAsync("room.list", new Dictionary<string, object?>());
    }

    private void HandleRoomJoined(string payloadJson)
    {
        var payload = DeserializePayload<RoomJoinedPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _roomId = payload.RoomId ?? string.Empty;
        _role = payload.Role ?? "player";
        _boardSize = payload.BoardSize > 0 ? payload.BoardSize : _boardSize;
        _phase = "Waiting";
        _targetRow = null;
        _targetCol = null;
        _pendingJoinRoomId = string.Empty;

        SetStatus($"Joined {_roomId} as {_role}");
        ShowLobbyScreen();
    }

    private void HandleRoomLeft()
    {
        ResetRoomState();
        ShowRoomListScreen();
        _ = SendMessageAsync("room.list", new Dictionary<string, object?>());
    }

    private void HandleRoomState(string payloadJson)
    {
        var payload = DeserializePayload<RoomStatePayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _roomId = payload.RoomId ?? _roomId;
        _phase = payload.Phase ?? _phase;
        _boardSize = payload.BoardSize > 0 ? payload.BoardSize : _boardSize;
        _players = payload.Players ?? new List<PlayerStateModel>();
        _observers = payload.Observers ?? new List<ObserverStateModel>();

        var mj = _players.FirstOrDefault(p => p.IsMj);
        if (mj is not null)
        {
            _mjSessionId = mj.SessionId ?? _mjSessionId;
        }

        _currentLobby?.SetRoomState(_roomId, _phase, _role, IsCurrentUserMj(), _players, _observers);

        if (_currentGame is not null)
        {
            RefreshGameScreen();
        }
    }

    private void HandleRoundStarted(string payloadJson)
    {
        var payload = DeserializePayload<RoundStartedPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _phase = "MjSelecting";
        _mjSessionId = payload.MjSessionId ?? _mjSessionId;
        _boardSize = payload.BoardSize > 0 ? payload.BoardSize : _boardSize;
        _targetRow = null;
        _targetCol = null;

        ShowGameScreen();
        RefreshGameScreen();
    }

    private void HandleRoundTarget(string payloadJson)
    {
        var payload = DeserializePayload<RoundTargetPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _phase = "Clicking";
        _targetRow = payload.Row;
        _targetCol = payload.Col;

        ShowGameScreen();
        RefreshGameScreen();
    }

    private void HandleRoundResult(string payloadJson)
    {
        var payload = DeserializePayload<RoundResultPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        _phase = "Waiting";
        _targetRow = null;
        _targetCol = null;
        _lastResult = payload;

        ShowResultScreen();
    }

    private void HandleError(string payloadJson)
    {
        var payload = DeserializePayload<ErrorPayloadModel>(payloadJson);
        if (payload is null)
        {
            return;
        }

        var code = payload.Code ?? "unknown";
        var message = payload.Message ?? "Unknown error";
        SetStatus($"Error {code}: {message}");
    }

    private bool IsCurrentUserMj()
    {
        return !string.IsNullOrWhiteSpace(_sessionId)
            && !string.IsNullOrWhiteSpace(_mjSessionId)
            && string.Equals(_sessionId, _mjSessionId, StringComparison.OrdinalIgnoreCase);
    }

    private async void OnLoginConnectRequested(string host, int port)
    {
        if (_network.IsConnected)
        {
            _network.Disconnect();
            return;
        }

        _currentLogin?.SetStatusText($"Connecting {host}:{port} ...");
        await _network.ConnectAsync(host, port);
    }

    private async void OnLoginRequested(string email, string password)
    {
        if (!_network.IsConnected)
        {
            SetStatus("Connect first.");
            return;
        }

        await SendMessageAsync("auth.login", new Dictionary<string, object?>
        {
            ["email"] = email,
            ["password"] = password
        });
    }

    private async void OnRefreshRoomsRequested()
    {
        await SendMessageAsync("room.list", new Dictionary<string, object?>());
    }

    private async void OnCreateRoomRequested(string roomName, int boardSize)
    {
        await SendMessageAsync("room.create", new Dictionary<string, object?>
        {
            ["roomName"] = roomName,
            ["boardSize"] = boardSize
        });
    }

    private void OnRoomSelectedForRole(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            return;
        }

        _pendingJoinRoomId = roomId;
        ShowRoleSelectScreen(roomId);
    }

    private void OnRoleSelectBack()
    {
        ShowRoomListScreen();
    }

    private async void OnRoleSelectConfirm(string roomId, string role)
    {
        await SendMessageAsync("room.join", new Dictionary<string, object?>
        {
            ["roomId"] = roomId,
            ["role"] = role
        });
    }

    private async void OnLobbyReadyRequested(bool ready)
    {
        if (!string.Equals(_role, "player", StringComparison.OrdinalIgnoreCase))
        {
            SetStatus("Only player can ready.");
            return;
        }

        await SendMessageAsync("room.ready", new Dictionary<string, object?>
        {
            ["ready"] = ready
        });
    }

    private async void OnLobbyLeaveRequested()
    {
        await SendMessageAsync("room.leave", new Dictionary<string, object?>());
    }

    private void OnLobbyOpenGameRequested()
    {
        ShowGameScreen();
        RefreshGameScreen();
    }

    private async void OnGameCellActionRequested(int row, int col)
    {
        if (!string.Equals(_role, "player", StringComparison.OrdinalIgnoreCase))
        {
            SetStatus("Observer cannot act.");
            return;
        }

        if (_phase == "MjSelecting" && IsCurrentUserMj())
        {
            await SendMessageAsync("game.mj.select", new Dictionary<string, object?>
            {
                ["row"] = row,
                ["col"] = col
            });
            return;
        }

        if (_phase == "Clicking" && !IsCurrentUserMj())
        {
            await SendMessageAsync("game.click", new Dictionary<string, object?>
            {
                ["row"] = row,
                ["col"] = col
            });
            return;
        }

        SetStatus("Current role/phase does not allow this action.");
    }

    private void OnGameBackToLobbyRequested()
    {
        ShowLobbyScreen();
    }

    private async void OnResultBackToRoomsRequested()
    {
        await SendMessageAsync("room.leave", new Dictionary<string, object?>());
        ShowRoomListScreen();
    }

    private async System.Threading.Tasks.Task<bool> SendMessageAsync(string type, Dictionary<string, object?> payload)
    {
        return await _network.SendMessageAsync(type, payload);
    }

    private void ShowLoginScreen()
    {
        var screen = InstantiateScreen<LoginScreen>(_loginScene, "LoginScreen");
        if (screen is null)
        {
            return;
        }
        SetScreen(screen);
        screen.ConnectRequested += OnLoginConnectRequested;
        screen.LoginRequested += OnLoginRequested;
        screen.SetConnectionState(_network.IsConnected);
        screen.SetStatusText(_network.IsConnected ? "Connected. Please login." : "Please connect first.");

        _currentLogin = screen;
        _currentRoomList = null;
        _currentRoleSelect = null;
        _currentLobby = null;
        _currentGame = null;
        _currentResult = null;
    }

    private void ShowRoomListScreen()
    {
        var screen = InstantiateScreen<RoomListScreen>(_roomListScene, "RoomListScreen");
        if (screen is null)
        {
            return;
        }
        SetScreen(screen);
        screen.RefreshRequested += OnRefreshRoomsRequested;
        screen.CreateRoomRequested += OnCreateRoomRequested;
        screen.JoinRequested += OnRoomSelectedForRole;
        screen.SetRooms(_rooms);
        screen.SetStatus($"Logged in as {_displayName}");
        if (!string.IsNullOrWhiteSpace(_pendingJoinRoomId))
        {
            screen.SetSelectedRoom(_pendingJoinRoomId);
        }

        _currentLogin = null;
        _currentRoomList = screen;
        _currentRoleSelect = null;
        _currentLobby = null;
        _currentGame = null;
        _currentResult = null;
    }

    private void ShowRoleSelectScreen(string roomId)
    {
        var screen = InstantiateScreen<RoleSelectScreen>(_roleSelectScene, "RoleSelectScreen");
        if (screen is null)
        {
            return;
        }
        SetScreen(screen);
        screen.BackRequested += OnRoleSelectBack;
        screen.ConfirmRequested += OnRoleSelectConfirm;
        screen.SetRoom(roomId);

        _currentLogin = null;
        _currentRoomList = null;
        _currentRoleSelect = screen;
        _currentLobby = null;
        _currentGame = null;
        _currentResult = null;
    }

    private void ShowLobbyScreen()
    {
        var screen = InstantiateScreen<LobbyScreen>(_lobbyScene, "LobbyScreen");
        if (screen is null)
        {
            return;
        }
        SetScreen(screen);
        screen.ReadyRequested += OnLobbyReadyRequested;
        screen.LeaveRequested += OnLobbyLeaveRequested;
        screen.OpenGameRequested += OnLobbyOpenGameRequested;
        screen.SetRoomState(_roomId, _phase, _role, IsCurrentUserMj(), _players, _observers);

        _currentLogin = null;
        _currentRoomList = null;
        _currentRoleSelect = null;
        _currentLobby = screen;
        _currentGame = null;
        _currentResult = null;
    }

    private void ShowGameScreen()
    {
        var screen = InstantiateScreen<GameScreen>(_gameScene, "GameScreen");
        if (screen is null)
        {
            return;
        }
        SetScreen(screen);
        screen.CellActionRequested += OnGameCellActionRequested;
        screen.BackToLobbyRequested += OnGameBackToLobbyRequested;
        screen.SetBoardSize(_boardSize);

        _currentLogin = null;
        _currentRoomList = null;
        _currentRoleSelect = null;
        _currentLobby = null;
        _currentGame = screen;
        _currentResult = null;
    }

    private void ShowResultScreen()
    {
        var screen = InstantiateScreen<ResultScreen>(_resultScene, "ResultScreen");
        if (screen is null)
        {
            return;
        }
        SetScreen(screen);
        screen.BackToRoomsRequested += OnResultBackToRoomsRequested;
        screen.SetRoundResult(_lastResult, _sessionId);

        _currentLogin = null;
        _currentRoomList = null;
        _currentRoleSelect = null;
        _currentLobby = null;
        _currentGame = null;
        _currentResult = screen;
    }

    private void RefreshGameScreen()
    {
        if (_currentGame is null)
        {
            return;
        }

        var isPlayer = string.Equals(_role, "player", StringComparison.OrdinalIgnoreCase);
        var isMj = IsCurrentUserMj();

        var canAct = false;
        var instruction = "Waiting...";

        if (!isPlayer)
        {
            instruction = "Observer mode: watch only.";
        }
        else if (_phase == "MjSelecting")
        {
            if (isMj)
            {
                canAct = true;
                instruction = "You are MJ. Select a target cell.";
            }
            else
            {
                instruction = "Waiting for MJ to choose a target.";
            }
        }
        else if (_phase == "Clicking")
        {
            if (isMj)
            {
                instruction = "MJ mode: waiting for players to click.";
            }
            else
            {
                canAct = true;
                instruction = "Click the target cell quickly.";
            }
        }

        _currentGame.SetBoardSize(_boardSize);
        _currentGame.SetGameState(_roomId, _phase, _role, isMj, instruction, _targetRow, _targetCol, canAct);
    }

    private void SetScreen(Control screen)
    {
        foreach (var child in _screenHost.GetChildren())
        {
            if (child is Node node)
            {
                node.QueueFree();
            }
        }

        _screenHost.AddChild(screen);
        screen.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        screen.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
    }

    private T? InstantiateScreen<T>(PackedScene scene, string sceneName) where T : Control
    {
        if (scene is null)
        {
            SetStatus($"Scene load failed: {sceneName}");
            Log($"Scene load failed: {sceneName}");
            return null;
        }

        var node = scene.Instantiate();
        if (node is not T typed)
        {
            SetStatus($"Scene cast failed: {sceneName}");
            Log($"Scene cast failed: {sceneName}, actual={node.GetType().Name}");
            node.QueueFree();
            return null;
        }

        return typed;
    }

    private T? DeserializePayload<T>(string payloadJson) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payloadJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            Log($"Payload parse error ({typeof(T).Name}): {ex.Message}");
            return null;
        }
    }

    private void SetStatus(string text)
    {
        _statusLabel.Text = $"Status: {text}";
    }

    private void Log(string text)
    {
        _logOutput.AppendText(text + "\n");
    }

    private void ResetRoomState()
    {
        _roomId = string.Empty;
        _role = "player";
        _phase = "Waiting";
        _boardSize = 5;
        _mjSessionId = string.Empty;
        _targetRow = null;
        _targetCol = null;
        _players = new List<PlayerStateModel>();
        _observers = new List<ObserverStateModel>();
        _lastResult = null;
        _pendingJoinRoomId = string.Empty;
    }
}
