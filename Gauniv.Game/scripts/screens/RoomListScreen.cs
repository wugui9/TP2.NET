using Godot;
using System;
using System.Collections.Generic;

namespace Gauniv.Game;

public partial class RoomListScreen : Control
{
    [Signal]
    public delegate void RefreshRequestedEventHandler();

    [Signal]
    public delegate void CreateRoomRequestedEventHandler(string roomName, int boardSize);

    [Signal]
    public delegate void JoinRequestedEventHandler(string roomId);

    private Label _statusLabel = default!;
    private Label _roomCountLabel = default!;
    private VBoxContainer _roomsFlow = default!;
    private Button _createFloatingButton = default!;
    private ColorRect _createOverlay = default!;
    private LineEdit _newRoomNameInput = default!;
    private SpinBox _boardSizeInput = default!;
    private Button _dialogCancelButton = default!;
    private Button _dialogCreateButton = default!;

    private readonly Dictionary<string, RoomSummaryModel> _roomsById = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, PanelContainer> _roomCards = new(StringComparer.OrdinalIgnoreCase);

    private string _pendingStatus = string.Empty;
    private bool _hasPendingStatus;
    private string _pendingSelectedRoomId = string.Empty;
    private bool _hasPendingSelectedRoom;
    private IReadOnlyList<RoomSummaryModel>? _pendingRooms;
    private string _selectedRoomId = string.Empty;

    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("Root/Stack/TopBar/TopRow/StatusLabel");
        _roomCountLabel = GetNode<Label>("Root/Stack/RoomsPanel/RoomsVBox/RoomCountLabel");
        _roomsFlow = GetNode<VBoxContainer>("Root/Stack/RoomsPanel/RoomsVBox/RoomsScroll/RoomsFlow");
        _createFloatingButton = GetNode<Button>("Root/Stack/TopBar/TopRow/CreateFloatingButton");
        _createOverlay = GetNode<ColorRect>("CreateOverlay");
        _newRoomNameInput = GetNode<LineEdit>("CreateOverlay/CreateCenter/CreateDialog/DialogStack/NewRoomNameInput");
        _boardSizeInput = GetNode<SpinBox>("CreateOverlay/CreateCenter/CreateDialog/DialogStack/BoardSizeInput");
        _dialogCancelButton = GetNode<Button>("CreateOverlay/CreateCenter/CreateDialog/DialogStack/DialogButtons/DialogCancelButton");
        _dialogCreateButton = GetNode<Button>("CreateOverlay/CreateCenter/CreateDialog/DialogStack/DialogButtons/DialogCreateButton");

        GetNode<Button>("Root/Stack/TopBar/TopRow/RefreshButton").Pressed += () => EmitSignal(SignalName.RefreshRequested);
        _createFloatingButton.Pressed += OpenCreateDialog;
        _dialogCancelButton.Pressed += CloseCreateDialog;
        _dialogCreateButton.Pressed += OnCreatePressed;
        _newRoomNameInput.TextSubmitted += _ => OnCreatePressed();

        _createOverlay.Visible = false;

        if (_hasPendingStatus)
        {
            SetStatus(_pendingStatus);
            _hasPendingStatus = false;
        }

        if (_hasPendingSelectedRoom)
        {
            SetSelectedRoom(_pendingSelectedRoomId);
            _hasPendingSelectedRoom = false;
        }

        if (_pendingRooms is not null)
        {
            SetRooms(_pendingRooms);
            _pendingRooms = null;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_createOverlay.Visible)
        {
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            CloseCreateDialog();
            GetViewport().SetInputAsHandled();
        }
    }

    public void SetStatus(string text)
    {
        if (_statusLabel is null)
        {
            _pendingStatus = text;
            _hasPendingStatus = true;
            return;
        }

        _statusLabel.Text = text;
    }

    public void SetSelectedRoom(string roomId)
    {
        if (_roomsFlow is null)
        {
            _pendingSelectedRoomId = roomId;
            _hasPendingSelectedRoom = true;
            return;
        }

        _selectedRoomId = roomId?.Trim() ?? string.Empty;
        UpdateCardHighlights();
    }

    public void SetRooms(IReadOnlyList<RoomSummaryModel> rooms)
    {
        if (_roomsFlow is null)
        {
            _pendingRooms = rooms;
            return;
        }

        foreach (var child in _roomsFlow.GetChildren())
        {
            if (child is Node node)
            {
                node.QueueFree();
            }
        }

        _roomsById.Clear();
        _roomCards.Clear();

        for (var i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            var roomId = room.RoomId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(roomId))
            {
                continue;
            }

            _roomsById[roomId] = room;

            var card = BuildRoomCard(room);
            _roomsFlow.AddChild(card);
            _roomCards[roomId] = card;
        }

        if (_roomCards.Count == 0)
        {
            var empty = new Label
            {
                Text = "No rooms yet. Create one to start.",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SelfModulate = new Color(0.85f, 0.92f, 0.98f)
            };
            _roomsFlow.AddChild(empty);
        }

        _roomCountLabel.Text = $"{_roomCards.Count} room(s) available";

        if (!string.IsNullOrWhiteSpace(_selectedRoomId) && !_roomCards.ContainsKey(_selectedRoomId))
        {
            _selectedRoomId = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_selectedRoomId) && _roomCards.Count > 0)
        {
            foreach (var roomId in _roomCards.Keys)
            {
                _selectedRoomId = roomId;
                break;
            }
        }

        UpdateCardHighlights();
    }

    private PanelContainer BuildRoomCard(RoomSummaryModel room)
    {
        var roomId = room.RoomId ?? string.Empty;
        var roomName = string.IsNullOrWhiteSpace(room.RoomName) ? roomId : room.RoomName;
        var phase = room.Phase ?? "Unknown";
        var maxPlayers = Math.Max(room.MaxPlayers, 4);
        var isFull = room.Players >= maxPlayers;
        var status = isFull ? "Full" : phase;

        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 92)
        };

        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 10);
        card.AddChild(row);

        var info = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        info.AddThemeConstantOverride("separation", 2);
        row.AddChild(info);

        var title = new Label
        {
            Text = $"{roomName}   [{roomId}]"
        };
        info.AddChild(title);

        var meta = new Label
        {
            Text = $"{status}  |  Players {room.Players}/{maxPlayers}  |  Obs {room.Observers}  |  Board {room.BoardSize}x{room.BoardSize}",
            SelfModulate = status switch
            {
                "Full" => new Color(1.0f, 0.74f, 0.74f),
                "Waiting" => new Color(0.76f, 0.95f, 1.0f),
                "MjSelecting" => new Color(0.90f, 0.82f, 1.0f),
                "Clicking" => new Color(0.84f, 1.0f, 0.84f),
                _ => new Color(0.9f, 0.95f, 1.0f)
            }
        };
        info.AddChild(meta);

        var joinButton = new Button
        {
            Text = isFull ? "Full" : "Join",
            CustomMinimumSize = new Vector2(110, 42),
            Disabled = isFull
        };
        joinButton.Pressed += () => OnJoinRoomPressed(roomId);
        row.AddChild(joinButton);

        return card;
    }

    private void OnJoinRoomPressed(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            return;
        }

        _selectedRoomId = roomId;
        UpdateCardHighlights();
        EmitSignal(SignalName.JoinRequested, roomId);
    }

    private void UpdateCardHighlights()
    {
        foreach (var kv in _roomCards)
        {
            var selected = string.Equals(kv.Key, _selectedRoomId, StringComparison.OrdinalIgnoreCase);
            kv.Value.SelfModulate = selected
                ? new Color(0.86f, 0.96f, 1.0f)
                : Colors.White;
        }
    }

    private void OpenCreateDialog()
    {
        _createOverlay.Visible = true;
        _newRoomNameInput.GrabFocus();
        _newRoomNameInput.CaretColumn = _newRoomNameInput.Text.Length;
    }

    private void CloseCreateDialog()
    {
        _createOverlay.Visible = false;
    }

    private void OnCreatePressed()
    {
        var roomName = _newRoomNameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(roomName))
        {
            roomName = "My Room";
            _newRoomNameInput.Text = roomName;
        }

        EmitSignal(SignalName.CreateRoomRequested, roomName, (int)_boardSizeInput.Value);
        _createOverlay.Visible = false;
    }
}
