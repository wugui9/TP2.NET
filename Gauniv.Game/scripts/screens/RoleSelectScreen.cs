using Godot;

namespace Gauniv.Game;

public partial class RoleSelectScreen : Control
{
    [Signal]
    public delegate void ConfirmRequestedEventHandler(string roomId, string role);

    [Signal]
    public delegate void BackRequestedEventHandler();

    private Label _roomLabel = default!;
    private Label _selectedRoleLabel = default!;
    private Button _playerButton = default!;
    private Button _observerButton = default!;
    private string _roomId = string.Empty;
    private string _selectedRole = "player";

    public override void _Ready()
    {
        _roomLabel = GetNode<Label>("Root/Center/Card/Stack/RoomLabel");
        _selectedRoleLabel = GetNode<Label>("Root/Center/Card/Stack/SelectedRoleLabel");
        _playerButton = GetNode<Button>("Root/Center/Card/Stack/RoleRow/PlayerButton");
        _observerButton = GetNode<Button>("Root/Center/Card/Stack/RoleRow/ObserverButton");

        _playerButton.Pressed += () => SetRole("player");
        _observerButton.Pressed += () => SetRole("observer");
        GetNode<Button>("Root/Center/Card/Stack/ButtonRow/ConfirmButton").Pressed += OnConfirmPressed;
        GetNode<Button>("Root/Center/Card/Stack/ButtonRow/BackButton").Pressed += () => EmitSignal(SignalName.BackRequested);

        _roomLabel.Text = $"Room: {_roomId}";
        SetRole(_selectedRole);
    }

    public void SetRoom(string roomId)
    {
        _roomId = roomId;
        if (IsNodeReady())
        {
            _roomLabel.Text = $"Room: {_roomId}";
        }
    }

    private void SetRole(string role)
    {
        _selectedRole = role == "observer" ? "observer" : "player";
        _selectedRoleLabel.Text = $"Selected: {_selectedRole}";

        if (_playerButton is null || _observerButton is null)
        {
            return;
        }

        _playerButton.Disabled = _selectedRole == "player";
        _observerButton.Disabled = _selectedRole == "observer";
    }

    private void OnConfirmPressed()
    {
        EmitSignal(SignalName.ConfirmRequested, _roomId, _selectedRole);
    }
}
