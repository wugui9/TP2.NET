using Godot;
using System.Collections.Generic;

namespace Gauniv.Game;

public partial class LobbyScreen : Control
{
    [Signal]
    public delegate void ReadyRequestedEventHandler(bool ready);

    [Signal]
    public delegate void LeaveRequestedEventHandler();

    [Signal]
    public delegate void OpenGameRequestedEventHandler();

    private Label _headerLabel = default!;
    private Label _phaseLabel = default!;
    private Label _roleHintLabel = default!;
    private Label _playersCountLabel = default!;
    private Label _observersCountLabel = default!;
    private ItemList _playersList = default!;
    private ItemList _observersList = default!;
    private CheckBox _readyCheck = default!;
    private Button _readyButton = default!;
    private PendingLobbyState? _pendingState;

    public override void _Ready()
    {
        _headerLabel = GetNode<Label>("Root/Stack/HeaderPanel/HeaderStack/HeaderLabel");
        _phaseLabel = GetNode<Label>("Root/Stack/HeaderPanel/HeaderStack/PhaseLabel");
        _roleHintLabel = GetNode<Label>("Root/Stack/MiddleRow/ControlPanel/ControlStack/RoleHintLabel");
        _playersCountLabel = GetNode<Label>("Root/Stack/MiddleRow/PlayersPanel/PlayersPane/PlayersCountLabel");
        _observersCountLabel = GetNode<Label>("Root/Stack/MiddleRow/ObserversPanel/ObserversPane/ObserversCountLabel");
        _playersList = GetNode<ItemList>("Root/Stack/MiddleRow/PlayersPanel/PlayersPane/PlayersList");
        _observersList = GetNode<ItemList>("Root/Stack/MiddleRow/ObserversPanel/ObserversPane/ObserversList");
        _readyCheck = GetNode<CheckBox>("Root/Stack/MiddleRow/ControlPanel/ControlStack/ReadyCheck");
        _readyButton = GetNode<Button>("Root/Stack/MiddleRow/ControlPanel/ControlStack/ReadyButton");

        _readyButton.Pressed += () => EmitSignal(SignalName.ReadyRequested, _readyCheck.ButtonPressed);
        _readyCheck.Toggled += OnReadyToggled;
        GetNode<Button>("Root/Stack/MiddleRow/ControlPanel/ControlStack/OpenGameButton").Pressed += () => EmitSignal(SignalName.OpenGameRequested);
        GetNode<Button>("Root/Stack/MiddleRow/ControlPanel/ControlStack/LeaveButton").Pressed += () => EmitSignal(SignalName.LeaveRequested);

        if (_pendingState is not null)
        {
            var state = _pendingState;
            _pendingState = null;
            SetRoomState(state.RoomId, state.Phase, state.Role, state.IsMj, state.Players, state.Observers);
        }
    }

    public void SetRoomState(string roomId, string phase, string role, bool isMj, IReadOnlyList<PlayerStateModel> players, IReadOnlyList<ObserverStateModel> observers)
    {
        if (_headerLabel is null)
        {
            _pendingState = new PendingLobbyState(roomId, phase, role, isMj, players, observers);
            return;
        }

        _headerLabel.Text = $"Room {roomId} | Role: {role}";
        _phaseLabel.Text = $"Phase: {phase}" + (isMj ? " | MJ" : string.Empty);

        var playerRole = role == "player";
        _readyCheck.Visible = playerRole;
        _readyButton.Visible = playerRole;

        _roleHintLabel.Text = playerRole
            ? (isMj ? "You are MJ. Wait for round start or open game screen." : "Set READY and wait for round start.")
            : "Observer mode: you can watch but cannot click.";
        _roleHintLabel.SelfModulate = playerRole
            ? new Color(0.80f, 0.97f, 1.0f)
            : new Color(1.0f, 0.90f, 0.78f);

        _readyCheck.Disabled = !playerRole;
        OnReadyToggled(_readyCheck.ButtonPressed);

        _playersList.Clear();
        foreach (var player in players)
        {
            var name = player.DisplayName ?? player.SessionId ?? "unknown";
            var tags = player.IsMj ? "[MJ]" : "[P]";
            var ready = player.IsReady ? "READY" : "NOT READY";
            var index = _playersList.GetItemCount();
            _playersList.AddItem($"{tags} {name}  -  {ready}");
            _playersList.SetItemCustomFgColor(index, player.IsMj
                ? new Color(1.0f, 0.90f, 0.72f)
                : (player.IsReady ? new Color(0.80f, 1.0f, 0.84f) : new Color(0.93f, 0.93f, 1.0f)));
        }

        _observersList.Clear();
        foreach (var observer in observers)
        {
            var name = observer.DisplayName ?? observer.SessionId ?? "unknown";
            var index = _observersList.GetItemCount();
            _observersList.AddItem($"[OBS] {name}");
            _observersList.SetItemCustomFgColor(index, new Color(0.84f, 0.92f, 1.0f));
        }

        _playersCountLabel.Text = $"{players.Count} player(s)";
        _observersCountLabel.Text = $"{observers.Count} observer(s)";
    }

    private void OnReadyToggled(bool isReady)
    {
        if (_readyButton is null)
        {
            return;
        }

        _readyButton.Text = isReady ? "Send READY" : "Send NOT READY";
    }

    private sealed record PendingLobbyState(
        string RoomId,
        string Phase,
        string Role,
        bool IsMj,
        IReadOnlyList<PlayerStateModel> Players,
        IReadOnlyList<ObserverStateModel> Observers);
}
