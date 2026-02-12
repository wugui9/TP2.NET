using Godot;

namespace Gauniv.Game;

public partial class RoleSelectScreen : Control
{
    [Signal]
    public delegate void ConfirmRequestedEventHandler(string roomId, string role);

    [Signal]
    public delegate void BackRequestedEventHandler();

    private Label _roomLabel = default!;
    private OptionButton _roleOption = default!;
    private string _roomId = string.Empty;

    public override void _Ready()
    {
        _roomLabel = GetNode<Label>("Root/Stack/RoomLabel");
        _roleOption = GetNode<OptionButton>("Root/Stack/RoleOption");

        _roleOption.Clear();
        _roleOption.AddItem("player");
        _roleOption.AddItem("observer");
        _roleOption.Selected = 0;

        GetNode<Button>("Root/Stack/ButtonRow/ConfirmButton").Pressed += OnConfirmPressed;
        GetNode<Button>("Root/Stack/ButtonRow/BackButton").Pressed += () => EmitSignal(SignalName.BackRequested);
    }

    public void SetRoom(string roomId)
    {
        _roomId = roomId;
        if (IsNodeReady())
        {
            _roomLabel.Text = $"Room: {_roomId}";
        }
    }

    public override void _EnterTree()
    {
        if (_roomLabel is not null)
        {
            _roomLabel.Text = $"Room: {_roomId}";
        }
    }

    private void OnConfirmPressed()
    {
        var role = _roleOption.GetItemText(_roleOption.Selected);
        EmitSignal(SignalName.ConfirmRequested, _roomId, role);
    }
}
