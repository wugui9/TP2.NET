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
    private string _pendingHost = "127.0.0.1";
    private int _pendingPort = 7000;
    private bool _hasPendingConnectionDefaults;

    public override void _Ready()
    {
        _hostInput = GetNode<LineEdit>("Root/Stack/CardsRow/ConnectionCard/ConnectionStack/ConnectionRow/HostInput");
        _portInput = GetNode<SpinBox>("Root/Stack/CardsRow/ConnectionCard/ConnectionStack/ConnectionRow/PortInput");
        _connectButton = GetNode<Button>("Root/Stack/CardsRow/ConnectionCard/ConnectionStack/ConnectButton");
        _emailInput = GetNode<LineEdit>("Root/Stack/CardsRow/AuthCard/AuthStack/AuthRow/EmailInput");
        _passwordInput = GetNode<LineEdit>("Root/Stack/CardsRow/AuthCard/AuthStack/AuthRow/PasswordInput");
        _loginButton = GetNode<Button>("Root/Stack/CardsRow/AuthCard/AuthStack/LoginButton");
        _statusLabel = GetNode<Label>("Root/Stack/StatusPanel/StatusLabel");

        _portInput.Value = 7000;

        _connectButton.Pressed += OnConnectPressed;
        _loginButton.Pressed += OnLoginPressed;
        _hostInput.TextSubmitted += _ => OnConnectPressed();
        _passwordInput.TextSubmitted += _ => OnLoginPressed();

        GetNode<Button>("Root/Stack/CardsRow/AuthCard/AuthStack/QuickRow/QuickP1Button").Pressed += () => FillAccount("p1@test.com", "password");
        GetNode<Button>("Root/Stack/CardsRow/AuthCard/AuthStack/QuickRow/QuickP2Button").Pressed += () => FillAccount("p2@test.com", "password");
        GetNode<Button>("Root/Stack/CardsRow/AuthCard/AuthStack/QuickRow/QuickP3Button").Pressed += () => FillAccount("p3@test.com", "password");
        GetNode<Button>("Root/Stack/CardsRow/AuthCard/AuthStack/QuickRow/QuickP4Button").Pressed += () => FillAccount("p4@test.com", "password");
        FillAccount("p1@test.com", "password");

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

        if (_hasPendingConnectionDefaults)
        {
            SetConnectionDefaults(_pendingHost, _pendingPort);
            _hasPendingConnectionDefaults = false;
        }
    }

    public void SetConnectionState(bool connected)
    {
        if (_connectButton is null || _loginButton is null)
        {
            _pendingConnected = connected;
            _hasPendingConnected = true;
            return;
        }

        _connectButton.Text = connected ? "Disconnect" : "Connect";
        _loginButton.Disabled = !connected;
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
        var lower = text.ToLowerInvariant();
        _statusLabel.SelfModulate = lower.Contains("error") || lower.Contains("failed")
            ? new Color(1.0f, 0.76f, 0.78f)
            : (lower.Contains("connect")
                ? new Color(0.80f, 0.96f, 1.0f)
                : new Color(0.90f, 0.96f, 1.0f));
    }

    public void SetConnectionDefaults(string host, int port)
    {
        if (_hostInput is null || _portInput is null)
        {
            _pendingHost = string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host.Trim();
            _pendingPort = port <= 0 ? 7000 : port;
            _hasPendingConnectionDefaults = true;
            return;
        }

        _hostInput.Text = string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host.Trim();
        _portInput.Value = port <= 0 ? 7000 : port;
    }

    private void FillAccount(string email, string password)
    {
        _emailInput.Text = email;
        _passwordInput.Text = password;
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
