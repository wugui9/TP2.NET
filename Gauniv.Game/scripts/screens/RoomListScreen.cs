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
    private ItemList _roomsList = default!;
    private LineEdit _selectedRoomInput = default!;
    private LineEdit _newRoomNameInput = default!;
    private SpinBox _boardSizeInput = default!;
    private string _pendingStatus = string.Empty;
    private bool _hasPendingStatus;
    private string _pendingSelectedRoom = string.Empty;
    private bool _hasPendingSelectedRoom;
    private IReadOnlyList<RoomSummaryModel>? _pendingRooms;

    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("Root/Stack/StatusLabel");
        _roomsList = GetNode<ItemList>("Root/Stack/RoomsList");
        _selectedRoomInput = GetNode<LineEdit>("Root/Stack/JoinRow/SelectedRoomInput");
        _newRoomNameInput = GetNode<LineEdit>("Root/Stack/CreateRow/NewRoomNameInput");
        _boardSizeInput = GetNode<SpinBox>("Root/Stack/CreateRow/BoardSizeInput");

        GetNode<Button>("Root/Stack/ActionRow/RefreshButton").Pressed += () => EmitSignal(SignalName.RefreshRequested);
        GetNode<Button>("Root/Stack/CreateRow/CreateButton").Pressed += OnCreatePressed;
        GetNode<Button>("Root/Stack/JoinRow/JoinButton").Pressed += OnJoinPressed;
        _roomsList.ItemSelected += OnRoomSelected;

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
    }

    public void SetRooms(IReadOnlyList<RoomSummaryModel> rooms)
    {
        if (_roomsList is null)
        {
            _pendingRooms = rooms;
            return;
        }
        _roomsList.Clear();

        for (var i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            var roomId = room.RoomId ?? string.Empty;
            var phase = room.Phase ?? "Unknown";
            var status = phase;
            if (room.Players >= room.MaxPlayers && room.MaxPlayers > 0)
            {
                status = "Full";
            }

            var text = $"{roomId} | {status} | {room.Players}/{Math.Max(room.MaxPlayers, 4)} | obs:{room.Observers}";
            var index = _roomsList.GetItemCount();
            _roomsList.AddItem(text);
            _roomsList.SetItemMetadata(index, roomId);
        }
    }

    private void OnRoomSelected(long index)
    {
        var meta = _roomsList.GetItemMetadata((int)index);
        if (meta.VariantType == Variant.Type.String)
        {
            _selectedRoomInput.Text = meta.AsString();
        }
    }

    private void OnCreatePressed()
    {
        EmitSignal(SignalName.CreateRoomRequested, _newRoomNameInput.Text.Trim(), (int)_boardSizeInput.Value);
    }

    private void OnJoinPressed()
    {
        EmitSignal(SignalName.JoinRequested, _selectedRoomInput.Text.Trim());
    }
}
