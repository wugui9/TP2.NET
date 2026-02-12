using Godot;

namespace Gauniv.Game;

public partial class LoginScreen : Control
{
    [Signal]
    public delegate void ConnectRequestedEventHandler(string host, int port);

    [Signal]
    public delegate void LoginRequestedEventHandler(string email, string password);

    private LineEdit _hostInput = default!;
    private SpinBox _portInput = default!;
    private Button _connectButton = default!;
    private LineEdit _emailInput = default!;
    private LineEdit _passwordInput = default!;
    private Button _loginButton = default!;
    private Label _statusLabel = default!;
    private bool _pendingConnected;
    private bool _hasPendingConnected;
    private string _pendingStatus = string.Empty;
    private bool _hasPendingStatus;

    public override void _Ready()
    {
        _hostInput = GetNode<LineEdit>("Root/Stack/ConnectionRow/HostInput");
        _portInput = GetNode<SpinBox>("Root/Stack/ConnectionRow/PortInput");
        _connectButton = GetNode<Button>("Root/Stack/ConnectionRow/ConnectButton");
        _emailInput = GetNode<LineEdit>("Root/Stack/AuthRow/EmailInput");
        _passwordInput = GetNode<LineEdit>("Root/Stack/AuthRow/PasswordInput");
        _loginButton = GetNode<Button>("Root/Stack/AuthRow/LoginButton");
        _statusLabel = GetNode<Label>("Root/Stack/StatusLabel");

        _connectButton.Pressed += OnConnectPressed;
        _loginButton.Pressed += OnLoginPressed;

        if (_hasPendingConnected)
        {
            SetConnectionState(_pendingConnected);
            _hasPendingConnected = false;
        }

        if (_hasPendingStatus)
        {
            SetStatusText(_pendingStatus);
            _hasPendingStatus = false;
        }
    }

    public void SetConnectionState(bool connected)
    {
        if (_connectButton is null)
        {
            _pendingConnected = connected;
            _hasPendingConnected = true;
            return;
        }
        _connectButton.Text = connected ? "Disconnect" : "Connect";
    }

    public void SetStatusText(string text)
    {
        if (_statusLabel is null)
        {
            _pendingStatus = text;
            _hasPendingStatus = true;
            return;
        }
        _statusLabel.Text = text;
    }

    private void OnConnectPressed()
    {
        EmitSignal(SignalName.ConnectRequested, _hostInput.Text.Trim(), (int)_portInput.Value);
    }

    private void OnLoginPressed()
    {
        EmitSignal(SignalName.LoginRequested, _emailInput.Text.Trim(), _passwordInput.Text);
    }
}
