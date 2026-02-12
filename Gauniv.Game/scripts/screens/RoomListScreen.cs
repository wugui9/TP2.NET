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
    private ItemList _roomsList = default!;
    private LineEdit _selectedRoomInput = default!;
    private Label _selectedRoomMetaLabel = default!;
    private LineEdit _newRoomNameInput = default!;
    private SpinBox _boardSizeInput = default!;
    private Button _joinButton = default!;

    private readonly Dictionary<string, RoomSummaryModel> _roomsById = new(StringComparer.OrdinalIgnoreCase);

    private string _pendingStatus = string.Empty;
    private bool _hasPendingStatus;
    private string _pendingSelectedRoom = string.Empty;
    private bool _hasPendingSelectedRoom;
    private IReadOnlyList<RoomSummaryModel>? _pendingRooms;

    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("Root/Stack/TopBar/TopRow/StatusLabel");
        _roomCountLabel = GetNode<Label>("Root/Stack/BodyRow/RoomsPanel/RoomsVBox/RoomCountLabel");
        _roomsList = GetNode<ItemList>("Root/Stack/BodyRow/RoomsPanel/RoomsVBox/RoomsList");
        _selectedRoomInput = GetNode<LineEdit>("Root/Stack/BodyRow/SidePanel/SideVBox/SelectedRoomInput");
        _selectedRoomMetaLabel = GetNode<Label>("Root/Stack/BodyRow/SidePanel/SideVBox/SelectedRoomMetaLabel");
        _newRoomNameInput = GetNode<LineEdit>("Root/Stack/BodyRow/SidePanel/SideVBox/NewRoomNameInput");
        _boardSizeInput = GetNode<SpinBox>("Root/Stack/BodyRow/SidePanel/SideVBox/CreateConfigRow/BoardSizeInput");
        _joinButton = GetNode<Button>("Root/Stack/BodyRow/SidePanel/SideVBox/JoinButton");

        GetNode<Button>("Root/Stack/TopBar/TopRow/RefreshButton").Pressed += () => EmitSignal(SignalName.RefreshRequested);
        GetNode<Button>("Root/Stack/BodyRow/SidePanel/SideVBox/CreateConfigRow/CreateButton").Pressed += OnCreatePressed;
        _joinButton.Pressed += OnJoinPressed;
        _roomsList.ItemSelected += OnRoomSelected;
        _joinButton.Disabled = true;

        if (_hasPendingStatus)
        {
            SetStatus(_pendingStatus);
            _hasPendingStatus = false;
        }

        if (_hasPendingSelectedRoom)
        {
            SetSelectedRoom(_pendingSelectedRoom);
            _hasPendingSelectedRoom = false;
        }

        if (_pendingRooms is not null)
        {
            SetRooms(_pendingRooms);
            _pendingRooms = null;
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
        if (_selectedRoomInput is null)
        {
            _pendingSelectedRoom = roomId;
            _hasPendingSelectedRoom = true;
            return;
        }

        _selectedRoomInput.Text = roomId;
        UpdateSelectedMeta(roomId);
    }

    public void SetRooms(IReadOnlyList<RoomSummaryModel> rooms)
    {
        if (_roomsList is null)
        {
            _pendingRooms = rooms;
            return;
        }

        _roomsById.Clear();
        _roomsList.Clear();

        for (var i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            var roomId = room.RoomId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(roomId))
            {
                continue;
            }

            _roomsById[roomId] = room;

            var roomName = string.IsNullOrWhiteSpace(room.RoomName) ? roomId : room.RoomName;
            var phase = room.Phase ?? "Unknown";
            var maxPlayers = Math.Max(room.MaxPlayers, 4);
            var status = room.Players >= maxPlayers ? "Full" : phase;

            var text = $"{roomName}  [{roomId}]  |  {status}  |  {room.Players}/{maxPlayers}  |  Obs {room.Observers}";
            var index = _roomsList.GetItemCount();
            _roomsList.AddItem(text);
            _roomsList.SetItemMetadata(index, roomId);

            var color = status switch
            {
                "Full" => new Color(1.0f, 0.74f, 0.74f),
                "Waiting" => new Color(0.76f, 0.95f, 1.0f),
                "MjSelecting" => new Color(0.90f, 0.82f, 1.0f),
                "Clicking" => new Color(0.84f, 1.0f, 0.84f),
                _ => new Color(0.9f, 0.95f, 1.0f)
            };
            _roomsList.SetItemCustomFgColor(index, color);
        }

        _roomCountLabel.Text = $"{_roomsById.Count} room(s) available";

        var current = _selectedRoomInput.Text.Trim();
        if (!string.IsNullOrWhiteSpace(current) && _roomsById.ContainsKey(current))
        {
            UpdateSelectedMeta(current);
        }
        else if (_roomsList.GetItemCount() > 0)
        {
            _roomsList.Select(0);
            OnRoomSelected(0);
        }
        else
        {
            _selectedRoomInput.Text = string.Empty;
            _selectedRoomMetaLabel.Text = "Status: -";
            _joinButton.Disabled = true;
        }
    }

    private void OnRoomSelected(long index)
    {
        var meta = _roomsList.GetItemMetadata((int)index);
        if (meta.VariantType == Variant.Type.String)
        {
            var roomId = meta.AsString();
            _selectedRoomInput.Text = roomId;
            UpdateSelectedMeta(roomId);
        }
    }

    private void UpdateSelectedMeta(string roomId)
    {
        if (!_roomsById.TryGetValue(roomId, out var room))
        {
            _selectedRoomMetaLabel.Text = "Status: unknown room";
            return;
        }

        var phase = room.Phase ?? "Unknown";
        var maxPlayers = Math.Max(room.MaxPlayers, 4);
        var status = room.Players >= maxPlayers ? "Full" : phase;
        _selectedRoomMetaLabel.Text = $"Status: {status} | Players {room.Players}/{maxPlayers} | Obs {room.Observers} | Board {room.BoardSize}x{room.BoardSize}";
        _joinButton.Disabled = status == "Full";
    }

    private void OnCreatePressed()
    {
        EmitSignal(SignalName.CreateRoomRequested, _newRoomNameInput.Text.Trim(), (int)_boardSizeInput.Value);
    }

    private void OnJoinPressed()
    {
        var roomId = _selectedRoomInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(roomId))
        {
            return;
        }

        EmitSignal(SignalName.JoinRequested, roomId);
    }
}
