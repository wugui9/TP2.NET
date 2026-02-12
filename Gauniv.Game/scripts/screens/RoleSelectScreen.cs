using Godot;

namespace Gauniv.Game;

public partial class RoleSelectScreen : Control
{
    [Signal]
    public delegate void ConfirmRequestedEventHandler(string roomId, string role);

    [Signal]
    public delegate void BackRequestedEventHandler();

    private Label _roomLabel = default!;
    private Button _playerButton = default!;
    private Button _observerButton = default!;
    private string _roomId = string.Empty;

    public override void _Ready()
    {
        _roomLabel = GetNode<Label>("Root/Stack/Center/Card/CardStack/RoomLabel");
        _playerButton = GetNode<Button>("Root/Stack/Center/Card/CardStack/RoleRow/PlayerButton");
        _observerButton = GetNode<Button>("Root/Stack/Center/Card/CardStack/RoleRow/ObserverButton");

        _playerButton.Pressed += () => JoinAsRole("player");
        _observerButton.Pressed += () => JoinAsRole("observer");
        GetNode<Button>("Root/Stack/TopBar/BackButton").Pressed += () => EmitSignal(SignalName.BackRequested);

        _roomLabel.Text = $"Room: {_roomId}";
    }

    public void SetRoom(string roomId)
    {
        _roomId = roomId;
        if (IsNodeReady())
        {
            _roomLabel.Text = $"Room: {_roomId}";
        }
    }

    private void JoinAsRole(string role)
    {
        if (string.IsNullOrWhiteSpace(_roomId))
        {
            return;
        }

        _playerButton.Disabled = true;
        _observerButton.Disabled = true;
        EmitSignal(SignalName.ConfirmRequested, _roomId, role);
    }
}
