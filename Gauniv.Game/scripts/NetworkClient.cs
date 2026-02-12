using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Gauniv.Game;

public partial class NetworkClient : Node
{
    [Signal]
    public delegate void ConnectedEventHandler();

    [Signal]
    public delegate void DisconnectedEventHandler();

    [Signal]
    public delegate void MessageReceivedEventHandler(string type, string payloadJson);

    [Signal]
    public delegate void TransportErrorEventHandler(string message);

    private readonly ConcurrentQueue<ClientEvent> _eventQueue = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly object _gate = new();
    private TcpClient? _tcpClient;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cts;
    private Task? _readerTask;

    public bool IsConnected
    {
        get
        {
            lock (_gate)
            {
                return _tcpClient is { Connected: true };
            }
        }
    }

    public async Task<bool> ConnectAsync(string host, int port)
    {
        Disconnect();

        try
        {
            var localTcpClient = new TcpClient();
            await localTcpClient.ConnectAsync(host, port);

            var localStream = localTcpClient.GetStream();
            var localReader = new StreamReader(localStream, Encoding.UTF8, false, 1024, leaveOpen: true);
            var localWriter = new StreamWriter(localStream, new UTF8Encoding(false), 1024, leaveOpen: true)
            {
                AutoFlush = true,
                NewLine = "\n"
            };

            lock (_gate)
            {
                _tcpClient = localTcpClient;
                _reader = localReader;
                _writer = localWriter;
                _cts = new CancellationTokenSource();
                _readerTask = Task.Run(() => ReaderLoopAsync(_cts.Token));
            }

            _eventQueue.Enqueue(ClientEvent.Connected());
            return true;
        }
        catch (Exception ex)
        {
            _eventQueue.Enqueue(ClientEvent.TransportError($"Connect failed: {ex.Message}"));
            Disconnect();
            return false;
        }
    }

    public async Task<bool> SendMessageAsync(string type, Dictionary<string, object?> payload)
    {
        StreamWriter? localWriter;
        lock (_gate)
        {
            localWriter = _writer;
        }

        if (localWriter is null)
        {
            _eventQueue.Enqueue(ClientEvent.TransportError("Send failed: socket is not connected."));
            return false;
        }

        try
        {
            var localEnvelope = new ClientEnvelope
            {
                Type = type,
                Payload = payload
            };

            var localJson = JsonSerializer.Serialize(localEnvelope, _jsonOptions);
            await localWriter.WriteLineAsync(localJson);
            return true;
        }
        catch (Exception ex)
        {
            _eventQueue.Enqueue(ClientEvent.TransportError($"Send failed: {ex.Message}"));
            return false;
        }
    }

    public void Disconnect()
    {
        CancellationTokenSource? localCts;
        StreamWriter? localWriter;
        StreamReader? localReader;
        TcpClient? localTcpClient;

        lock (_gate)
        {
            localCts = _cts;
            localWriter = _writer;
            localReader = _reader;
            localTcpClient = _tcpClient;

            _cts = null;
            _writer = null;
            _reader = null;
            _tcpClient = null;
            _readerTask = null;
        }

        try { localCts?.Cancel(); } catch { }
        try { localWriter?.Dispose(); } catch { }
        try { localReader?.Dispose(); } catch { }
        try { localTcpClient?.Close(); } catch { }
    }

    public override void _Process(double delta)
    {
        while (_eventQueue.TryDequeue(out var localEvent))
        {
            switch (localEvent.Kind)
            {
                case ClientEventKind.Connected:
                    EmitSignal(SignalName.Connected);
                    break;
                case ClientEventKind.Disconnected:
                    EmitSignal(SignalName.Disconnected);
                    break;
                case ClientEventKind.Message:
                    EmitSignal(SignalName.MessageReceived, localEvent.Type ?? string.Empty, localEvent.PayloadJson ?? "{}");
                    break;
                case ClientEventKind.TransportError:
                    EmitSignal(SignalName.TransportError, localEvent.ErrorMessage ?? "Unknown transport error.");
                    break;
            }
        }
    }

    private async Task ReaderLoopAsync(CancellationToken token)
    {
        StreamReader? localReader;
        lock (_gate)
        {
            localReader = _reader;
        }

        if (localReader is null)
        {
            _eventQueue.Enqueue(ClientEvent.TransportError("Reader loop could not start."));
            return;
        }

        try
        {
            while (!token.IsCancellationRequested)
            {
                var localLine = await localReader.ReadLineAsync();
                if (localLine is null)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(localLine))
                {
                    continue;
                }

                try
                {
                    using var localDoc = JsonDocument.Parse(localLine);
                    var localRoot = localDoc.RootElement;

                    if (!localRoot.TryGetProperty("type", out var localTypeProp))
                    {
                        continue;
                    }

                    var localType = localTypeProp.GetString() ?? string.Empty;
                    var localPayloadJson = "{}";
                    if (localRoot.TryGetProperty("payload", out var localPayloadProp))
                    {
                        localPayloadJson = localPayloadProp.GetRawText();
                    }

                    _eventQueue.Enqueue(ClientEvent.Message(localType, localPayloadJson));
                }
                catch (Exception ex)
                {
                    _eventQueue.Enqueue(ClientEvent.TransportError($"Invalid message: {ex.Message}"));
                }
            }
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
            {
                _eventQueue.Enqueue(ClientEvent.TransportError($"Connection lost: {ex.Message}"));
            }
        }
        finally
        {
            Disconnect();
            _eventQueue.Enqueue(ClientEvent.Disconnected());
        }
    }

    private sealed class ClientEnvelope
    {
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object?> Payload { get; set; } = new();
    }

    private readonly record struct ClientEvent(
        ClientEventKind Kind,
        string? Type,
        string? PayloadJson,
        string? ErrorMessage)
    {
        public static ClientEvent Connected() => new(ClientEventKind.Connected, null, null, null);
        public static ClientEvent Disconnected() => new(ClientEventKind.Disconnected, null, null, null);
        public static ClientEvent Message(string type, string payloadJson) => new(ClientEventKind.Message, type, payloadJson, null);
        public static ClientEvent TransportError(string message) => new(ClientEventKind.TransportError, null, null, message);
    }

    private enum ClientEventKind
    {
        Connected,
        Disconnected,
        Message,
        TransportError
    }
}
