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
    private ItemList _playersList = default!;
    private ItemList _observersList = default!;
    private CheckBox _readyCheck = default!;
    private Button _readyButton = default!;
    private LobbyViewState? _pendingState;

    public override void _Ready()
    {
        _headerLabel = GetNode<Label>("Root/Stack/HeaderLabel");
        _phaseLabel = GetNode<Label>("Root/Stack/PhaseLabel");
        _playersList = GetNode<ItemList>("Root/Stack/Lists/PlayersList");
        _observersList = GetNode<ItemList>("Root/Stack/Lists/ObserversList");
        _readyCheck = GetNode<CheckBox>("Root/Stack/ActionRow/ReadyCheck");
        _readyButton = GetNode<Button>("Root/Stack/ActionRow/ReadyButton");

        _readyButton.Pressed += () => EmitSignal(SignalName.ReadyRequested, _readyCheck.ButtonPressed);
        GetNode<Button>("Root/Stack/ActionRow/OpenGameButton").Pressed += () => EmitSignal(SignalName.OpenGameRequested);
        GetNode<Button>("Root/Stack/ActionRow/LeaveButton").Pressed += () => EmitSignal(SignalName.LeaveRequested);

        if (_pendingState is not null)
        {
            var state = _pendingState;
            _pendingState = null;
            SetRoomState(state.RoomId, state.Phase, state.Role, state.IsMj, state.Players, state.Observers);
        }
    }

    public void SetRoomState(string roomId, string phase, string role, bool isMj, IReadOnlyList<PlayerStateModel> players, IReadOnlyList<ObserverStateModel> observers)
    {
        if (_headerLabel is null || _phaseLabel is null || _playersList is null || _observersList is null || _readyCheck is null || _readyButton is null)
        {
            _pendingState = new LobbyViewState(roomId, phase, role, isMj, players, observers);
            return;
        }

        _headerLabel.Text = $"Room: {roomId} | Role: {role}";
        _phaseLabel.Text = $"Phase: {phase}" + (isMj ? " | You are MJ" : string.Empty);

        var playerRole = role == "player";
        _readyCheck.Visible = playerRole;
        _readyButton.Visible = playerRole;

        _playersList.Clear();
        foreach (var player in players)
        {
            var name = player.DisplayName ?? player.SessionId ?? "unknown";
            _playersList.AddItem($"{name} | ready:{player.IsReady} | mj:{player.IsMj}");
        }

        _observersList.Clear();
        foreach (var observer in observers)
        {
            var name = observer.DisplayName ?? observer.SessionId ?? "unknown";
            _observersList.AddItem(name);
        }
    }

    private sealed record LobbyViewState(
        string RoomId,
        string Phase,
        string Role,
        bool IsMj,
        IReadOnlyList<PlayerStateModel> Players,
        IReadOnlyList<ObserverStateModel> Observers);
}
