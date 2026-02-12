using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Gauniv.GameServer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var local_port = ReadInt("GAUNIV_GAMESERVER_PORT", 7000);
        var local_boardSize = ReadInt("GAUNIV_BOARD_SIZE", 5);
        var local_roomCount = Math.Max(4, ReadInt("GAUNIV_ROOM_COUNT", 4));
        var local_clickTimeoutSeconds = ReadInt("GAUNIV_CLICK_TIMEOUT_SECONDS", 10);
        var local_authBaseUrl = Environment.GetEnvironmentVariable("GAUNIV_AUTH_BASEURL") ?? "http://localhost:5231";

        var local_options = new GameServerOptions(
            Port: local_port,
            BoardSize: local_boardSize,
            RoomCount: local_roomCount,
            ClickTimeoutSeconds: local_clickTimeoutSeconds,
            AuthBaseUrl: local_authBaseUrl);

        var local_server = new GameTcpServer(local_options);
        Console.WriteLine($"[GameServer] Listening on tcp://0.0.0.0:{local_options.Port}");
        Console.WriteLine($"[GameServer] Rooms={local_options.RoomCount}, Board={local_options.BoardSize}x{local_options.BoardSize}");
        await local_server.RunAsync();
    }

    private static int ReadInt(string key, int fallback)
    {
        var local_text = Environment.GetEnvironmentVariable(key);
        return int.TryParse(local_text, out var local_value) ? local_value : fallback;
    }
}

public sealed record GameServerOptions(
    int Port,
    int BoardSize,
    int RoomCount,
    int ClickTimeoutSeconds,
    string AuthBaseUrl);

public enum RoomPhase
{
    Waiting,
    MjSelecting,
    Clicking
}

public enum ClientRole
{
    Player,
    Observer
}

public sealed class GameTcpServer
{
    private readonly GameServerOptions local_options;
    private readonly TcpListener local_listener;
    private readonly ConcurrentDictionary<string, ClientSession> local_sessions = new();
    private readonly ConcurrentDictionary<string, GameRoom> local_rooms = new();
    private readonly AuthGateway local_authGateway;
    private int local_roomSequence;

    public GameTcpServer(GameServerOptions options)
    {
        local_options = options;
        local_listener = new TcpListener(IPAddress.Any, options.Port);
        local_authGateway = new AuthGateway(options.AuthBaseUrl);
        local_roomSequence = options.RoomCount;

        for (var i = 1; i <= options.RoomCount; i++)
        {
            var local_room = new GameRoom($"room-{i}", $"Room {i}", options.BoardSize);
            local_rooms.TryAdd(local_room.Id, local_room);
        }
    }

    public async Task RunAsync()
    {
        local_listener.Start();

        while (true)
        {
            var local_tcpClient = await local_listener.AcceptTcpClientAsync();
            _ = Task.Run(async () => await HandleClientAsync(local_tcpClient));
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        var local_session = new ClientSession(tcpClient);
        local_sessions.TryAdd(local_session.SessionId, local_session);

        try
        {
            await local_session.SendAsync("server.hello", new
            {
                sessionId = local_session.SessionId,
                message = "Connected. Please authenticate.",
                protocol = "json-line-v1"
            });

            while (true)
            {
                var local_line = await local_session.ReadLineAsync();
                if (local_line is null)
                {
                    break;
                }

                ClientEnvelope? local_envelope;
                try
                {
                    local_envelope = JsonSerializer.Deserialize<ClientEnvelope>(local_line, JsonDefaults.SerializerOptions);
                }
                catch
                {
                    await local_session.SendErrorAsync("invalid_json", "Invalid JSON payload.");
                    continue;
                }

                if (local_envelope is null || string.IsNullOrWhiteSpace(local_envelope.Type))
                {
                    await local_session.SendErrorAsync("invalid_message", "Message must include type.");
                    continue;
                }

                await DispatchAsync(local_session, local_envelope);
            }
        }
        finally
        {
            await DisconnectAsync(local_session);
        }
    }

    private async Task DispatchAsync(ClientSession session, ClientEnvelope envelope)
    {
        switch (envelope.Type)
        {
            case "auth.login":
                await HandleAuthLoginAsync(session, envelope.Payload);
                return;
            case "room.list":
                await HandleRoomListAsync(session);
                return;
            case "room.join":
                await HandleJoinRoomAsync(session, envelope.Payload);
                return;
            case "room.create":
                await HandleCreateRoomAsync(session, envelope.Payload);
                return;
            case "room.leave":
                await HandleLeaveRoomAsync(session);
                return;
            case "room.ready":
                await HandleReadyAsync(session, envelope.Payload);
                return;
            case "game.mj.select":
                await HandleMjSelectAsync(session, envelope.Payload);
                return;
            case "game.click":
                await HandlePlayerClickAsync(session, envelope.Payload);
                return;
            case "ping":
                await session.SendAsync("pong", new { nowUtc = DateTime.UtcNow });
                return;
            default:
                await session.SendErrorAsync("unknown_type", $"Unknown message type '{envelope.Type}'.");
                return;
        }
    }

    private async Task HandleAuthLoginAsync(ClientSession session, JsonElement payload)
    {
        if (session.IsAuthenticated)
        {
            await session.SendErrorAsync("already_authenticated", "Session is already authenticated.");
            return;
        }

        LoginPayload? local_input;
        try
        {
            local_input = payload.Deserialize<LoginPayload>(JsonDefaults.SerializerOptions);
        }
        catch
        {
            await session.SendErrorAsync("invalid_payload", "auth.login payload is invalid.");
            return;
        }

        if (local_input is null || string.IsNullOrWhiteSpace(local_input.Email) || string.IsNullOrWhiteSpace(local_input.Password))
        {
            await session.SendErrorAsync("missing_credentials", "Email and password are required.");
            return;
        }

        var local_result = await local_authGateway.LoginAsync(local_input.Email, local_input.Password);
        if (!local_result.Success)
        {
            await session.SendErrorAsync("auth_failed", local_result.Message ?? "Authentication failed.");
            return;
        }

        session.AuthToken = Guid.NewGuid().ToString("N");
        session.Email = local_result.Email;
        session.DisplayName = string.IsNullOrWhiteSpace(local_result.DisplayName) ? local_result.Email : local_result.DisplayName;

        await session.SendAsync("auth.ok", new
        {
            token = session.AuthToken,
            email = session.Email,
            displayName = session.DisplayName
        });

        await BroadcastRoomListAsync();
    }

    private async Task HandleRoomListAsync(ClientSession session)
    {
        if (!session.IsAuthenticated)
        {
            await session.SendErrorAsync("not_authenticated", "Authenticate first.");
            return;
        }

        await session.SendAsync("room.list", BuildRoomListPayload());
    }

    private async Task HandleJoinRoomAsync(ClientSession session, JsonElement payload)
    {
        if (!session.IsAuthenticated)
        {
            await session.SendErrorAsync("not_authenticated", "Authenticate first.");
            return;
        }

        JoinRoomPayload? local_input;
        try
        {
            local_input = payload.Deserialize<JoinRoomPayload>(JsonDefaults.SerializerOptions);
        }
        catch
        {
            await session.SendErrorAsync("invalid_payload", "room.join payload is invalid.");
            return;
        }

        if (local_input is null || string.IsNullOrWhiteSpace(local_input.RoomId) || string.IsNullOrWhiteSpace(local_input.Role))
        {
            await session.SendErrorAsync("invalid_payload", "roomId and role are required.");
            return;
        }

        if (!local_rooms.TryGetValue(local_input.RoomId, out var local_room))
        {
            await session.SendErrorAsync("room_not_found", "Room does not exist.");
            return;
        }

        var local_role = local_input.Role.Equals("observer", StringComparison.OrdinalIgnoreCase)
            ? ClientRole.Observer
            : ClientRole.Player;

        if (session.RoomId is not null)
        {
            await RemoveFromCurrentRoomAsync(session);
        }

        string? local_error = null;
        lock (local_room.Gate)
        {
            if (local_role == ClientRole.Player && local_room.PlayerSessionIds.Count >= 4)
            {
                local_error = "Room already has 4 players.";
            }
            else
            {
                if (local_role == ClientRole.Player)
                {
                    local_room.PlayerSessionIds.Add(session.SessionId);
                }
                else
                {
                    local_room.ObserverSessionIds.Add(session.SessionId);
                }

                session.RoomId = local_room.Id;
                session.Role = local_role;
                session.IsReady = false;
            }
        }

        if (local_error is not null)
        {
            await session.SendErrorAsync("room_full", local_error);
            return;
        }

        await session.SendAsync("room.joined", new
        {
            roomId = local_room.Id,
            role = local_role.ToString().ToLowerInvariant(),
            boardSize = local_room.BoardSize
        });

        await BroadcastRoomStateAsync(local_room);
        await BroadcastRoomListAsync();
    }

    private async Task HandleCreateRoomAsync(ClientSession session, JsonElement payload)
    {
        if (!session.IsAuthenticated)
        {
            await session.SendErrorAsync("not_authenticated", "Authenticate first.");
            return;
        }

        CreateRoomPayload local_input = new();
        if (payload.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null)
        {
            try
            {
                local_input = payload.Deserialize<CreateRoomPayload>(JsonDefaults.SerializerOptions) ?? new CreateRoomPayload();
            }
            catch
            {
                await session.SendErrorAsync("invalid_payload", "room.create payload is invalid.");
                return;
            }
        }

        var local_boardSize = local_options.BoardSize;
        if (local_input.BoardSize is >= 3 and <= 9)
        {
            local_boardSize = local_input.BoardSize.Value;
        }

        GameRoom local_room;
        while (true)
        {
            var local_sequence = Interlocked.Increment(ref local_roomSequence);
            var local_roomId = $"room-{local_sequence}";
            var local_roomName = string.IsNullOrWhiteSpace(local_input.RoomName)
                ? $"Room {local_sequence}"
                : local_input.RoomName.Trim();

            local_room = new GameRoom(local_roomId, local_roomName, local_boardSize);
            if (local_rooms.TryAdd(local_room.Id, local_room))
            {
                break;
            }
        }

        await session.SendAsync("room.created", new
        {
            roomId = local_room.Id,
            roomName = local_room.Name,
            boardSize = local_room.BoardSize,
            maxPlayers = 4
        });

        await BroadcastRoomListAsync();
    }

    private async Task HandleLeaveRoomAsync(ClientSession session)
    {
        await RemoveFromCurrentRoomAsync(session);
        await session.SendAsync("room.left", new { });
    }

    private async Task HandleReadyAsync(ClientSession session, JsonElement payload)
    {
        if (!session.IsAuthenticated || session.RoomId is null || session.Role != ClientRole.Player)
        {
            await session.SendErrorAsync("forbidden", "Only player in room can set ready.");
            return;
        }

        ReadyPayload? local_input;
        try
        {
            local_input = payload.Deserialize<ReadyPayload>(JsonDefaults.SerializerOptions);
        }
        catch
        {
            await session.SendErrorAsync("invalid_payload", "room.ready payload is invalid.");
            return;
        }

        if (local_input is null)
        {
            await session.SendErrorAsync("invalid_payload", "ready flag is required.");
            return;
        }

        var local_room = local_rooms[session.RoomId];
        lock (local_room.Gate)
        {
            if (local_room.Phase != RoomPhase.Waiting)
            {
                return;
            }

            session.IsReady = local_input.Ready;
            if (local_input.Ready)
            {
                local_room.ReadyPlayerSessionIds.Add(session.SessionId);
            }
            else
            {
                local_room.ReadyPlayerSessionIds.Remove(session.SessionId);
            }
        }

        await BroadcastRoomStateAsync(local_room);
        await TryStartRoundAsync(local_room);
    }

    private async Task HandleMjSelectAsync(ClientSession session, JsonElement payload)
    {
        if (!session.IsAuthenticated || session.RoomId is null)
        {
            await session.SendErrorAsync("not_in_room", "Join a room first.");
            return;
        }

        var local_room = local_rooms[session.RoomId];
        MjSelectPayload? local_input;
        try
        {
            local_input = payload.Deserialize<MjSelectPayload>(JsonDefaults.SerializerOptions);
        }
        catch
        {
            await session.SendErrorAsync("invalid_payload", "game.mj.select payload is invalid.");
            return;
        }

        if (local_input is null)
        {
            await session.SendErrorAsync("invalid_payload", "row and col are required.");
            return;
        }

        bool local_accepted;
        lock (local_room.Gate)
        {
            local_accepted = local_room.Phase == RoomPhase.MjSelecting
                && local_room.MjSessionId == session.SessionId
                && local_input.Row >= 0
                && local_input.Row < local_room.BoardSize
                && local_input.Col >= 0
                && local_input.Col < local_room.BoardSize;

            if (local_accepted)
            {
                local_room.TargetRow = local_input.Row;
                local_room.TargetCol = local_input.Col;
                local_room.ClicksMsByPlayerSessionId.Clear();
                local_room.ClickStartUtc = DateTime.UtcNow;
                local_room.Phase = RoomPhase.Clicking;
            }
        }

        if (!local_accepted)
        {
            await session.SendErrorAsync("invalid_mj_action", "Only current MJ can select a valid cell.");
            return;
        }

        await BroadcastRoomEventAsync(local_room, "round.target", new
        {
            row = local_input.Row,
            col = local_input.Col,
            clickTimeoutSeconds = local_options.ClickTimeoutSeconds
        });

        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(local_options.ClickTimeoutSeconds));
            await FinalizeRoundAsync(local_room);
        });
    }

    private async Task HandlePlayerClickAsync(ClientSession session, JsonElement payload)
    {
        if (!session.IsAuthenticated || session.RoomId is null || session.Role != ClientRole.Player)
        {
            await session.SendErrorAsync("forbidden", "Only players can click.");
            return;
        }

        var local_room = local_rooms[session.RoomId];
        ClickPayload? local_input;
        try
        {
            local_input = payload.Deserialize<ClickPayload>(JsonDefaults.SerializerOptions);
        }
        catch
        {
            await session.SendErrorAsync("invalid_payload", "game.click payload is invalid.");
            return;
        }

        if (local_input is null)
        {
            await session.SendErrorAsync("invalid_payload", "row and col are required.");
            return;
        }

        bool local_isFinished;
        bool local_validClick;

        lock (local_room.Gate)
        {
            local_validClick = local_room.Phase == RoomPhase.Clicking
                && local_room.MjSessionId != session.SessionId
                && local_room.TargetRow == local_input.Row
                && local_room.TargetCol == local_input.Col
                && !local_room.ClicksMsByPlayerSessionId.ContainsKey(session.SessionId)
                && local_room.ClickStartUtc is not null;

            var local_clickStartUtc = local_room.ClickStartUtc;
            if (local_validClick && local_clickStartUtc is not null)
            {
                var local_ms = (long)(DateTime.UtcNow - local_clickStartUtc.Value).TotalMilliseconds;
                local_room.ClicksMsByPlayerSessionId[session.SessionId] = Math.Max(local_ms, 0);
            }

            var local_expectedPlayers = local_room.PlayerSessionIds.Where(local_id => local_id != local_room.MjSessionId).ToList();
            local_isFinished = local_room.Phase == RoomPhase.Clicking
                && local_expectedPlayers.All(local_id => local_room.ClicksMsByPlayerSessionId.ContainsKey(local_id));
        }

        if (!local_validClick)
        {
            await session.SendErrorAsync("click_rejected", "Click rejected by room state/rules.");
            return;
        }

        await session.SendAsync("click.accepted", new { });

        if (local_isFinished)
        {
            await FinalizeRoundAsync(local_room);
        }
    }

    private async Task TryStartRoundAsync(GameRoom room)
    {
        string? local_mjSessionId = null;

        lock (room.Gate)
        {
            if (room.Phase != RoomPhase.Waiting)
            {
                return;
            }

            if (room.PlayerSessionIds.Count < 2)
            {
                return;
            }

            if (!room.PlayerSessionIds.All(local_id => room.ReadyPlayerSessionIds.Contains(local_id)))
            {
                return;
            }

            local_mjSessionId = room.PlayerSessionIds.ElementAt(Random.Shared.Next(room.PlayerSessionIds.Count));
            room.MjSessionId = local_mjSessionId;
            room.TargetRow = null;
            room.TargetCol = null;
            room.ClickStartUtc = null;
            room.ClicksMsByPlayerSessionId.Clear();
            room.Phase = RoomPhase.MjSelecting;
        }

        await BroadcastRoomEventAsync(room, "round.started", new
        {
            mjSessionId = local_mjSessionId,
            mjDisplayName = local_mjSessionId is null ? null : ResolveDisplayName(local_mjSessionId),
            boardSize = room.BoardSize
        });

        await BroadcastRoomStateAsync(room);
    }

    private async Task FinalizeRoundAsync(GameRoom room)
    {
        RoomRoundResultPayload? local_payload = null;

        lock (room.Gate)
        {
            if (room.Phase != RoomPhase.Clicking)
            {
                return;
            }

            var local_players = room.PlayerSessionIds.Where(local_id => local_id != room.MjSessionId).ToList();
            var local_entries = new List<RoomRoundResultEntry>();

            foreach (var local_playerSessionId in local_players)
            {
                var local_hasClick = room.ClicksMsByPlayerSessionId.TryGetValue(local_playerSessionId, out var local_ms);
                local_entries.Add(new RoomRoundResultEntry
                {
                    SessionId = local_playerSessionId,
                    DisplayName = ResolveDisplayName(local_playerSessionId),
                    IsValid = local_hasClick,
                    ReactionMs = local_hasClick ? local_ms : null
                });
            }

            var local_ranked = local_entries
                .Where(local_e => local_e.IsValid && local_e.ReactionMs.HasValue)
                .OrderBy(local_e => local_e.ReactionMs)
                .ToList();

            for (var i = 0; i < local_ranked.Count; i++)
            {
                local_ranked[i].Rank = i + 1;
            }

            local_payload = new RoomRoundResultPayload
            {
                MjSessionId = room.MjSessionId,
                MjDisplayName = room.MjSessionId is null ? null : ResolveDisplayName(room.MjSessionId),
                Results = local_entries
            };

            room.Phase = RoomPhase.Waiting;
            room.MjSessionId = null;
            room.TargetRow = null;
            room.TargetCol = null;
            room.ClickStartUtc = null;
            room.ClicksMsByPlayerSessionId.Clear();
            room.ReadyPlayerSessionIds.Clear();

            foreach (var local_sessionId in room.PlayerSessionIds)
            {
                if (local_sessions.TryGetValue(local_sessionId, out var local_session))
                {
                    local_session.IsReady = false;
                }
            }
        }

        if (local_payload is not null)
        {
            await BroadcastRoomEventAsync(room, "round.result", local_payload);
            await BroadcastRoomStateAsync(room);
            await BroadcastRoomListAsync();
        }
    }

    private async Task RemoveFromCurrentRoomAsync(ClientSession session)
    {
        if (session.RoomId is null)
        {
            return;
        }

        if (!local_rooms.TryGetValue(session.RoomId, out var local_room))
        {
            session.RoomId = null;
            session.Role = null;
            session.IsReady = false;
            return;
        }

        lock (local_room.Gate)
        {
            local_room.PlayerSessionIds.Remove(session.SessionId);
            local_room.ObserverSessionIds.Remove(session.SessionId);
            local_room.ReadyPlayerSessionIds.Remove(session.SessionId);
            local_room.ClicksMsByPlayerSessionId.Remove(session.SessionId);

            if (local_room.MjSessionId == session.SessionId)
            {
                local_room.Phase = RoomPhase.Waiting;
                local_room.MjSessionId = null;
                local_room.TargetRow = null;
                local_room.TargetCol = null;
                local_room.ClickStartUtc = null;
                local_room.ClicksMsByPlayerSessionId.Clear();
                local_room.ReadyPlayerSessionIds.Clear();
            }
        }

        session.RoomId = null;
        session.Role = null;
        session.IsReady = false;

        await BroadcastRoomStateAsync(local_room);
        await BroadcastRoomListAsync();
    }

    private async Task DisconnectAsync(ClientSession session)
    {
        await RemoveFromCurrentRoomAsync(session);
        local_sessions.TryRemove(session.SessionId, out _);
        session.Dispose();
        await BroadcastRoomListAsync();
    }

    private object BuildRoomListPayload()
    {
        var local_list = local_rooms.Values
            .OrderBy(local_room => local_room.Id)
            .Select(local_room => new
            {
                roomId = local_room.Id,
                roomName = local_room.Name,
                phase = local_room.Phase.ToString(),
                players = local_room.PlayerSessionIds.Count,
                observers = local_room.ObserverSessionIds.Count,
                maxPlayers = 4,
                boardSize = local_room.BoardSize
            })
            .ToList();

        return new { rooms = local_list };
    }

    private async Task BroadcastRoomListAsync()
    {
        var local_payload = BuildRoomListPayload();
        var local_targets = local_sessions.Values.Where(local_s => local_s.IsAuthenticated).ToList();

        foreach (var local_session in local_targets)
        {
            try
            {
                await local_session.SendAsync("room.list", local_payload);
            }
            catch
            {
                // Ignore transient/broken socket during broadcast so one dead client
                // does not break the whole room flow.
            }
        }
    }

    private async Task BroadcastRoomStateAsync(GameRoom room)
    {
        var local_payload = BuildRoomStatePayload(room);

        foreach (var local_sessionId in room.GetAllSessionIds())
        {
            if (!local_sessions.TryGetValue(local_sessionId, out var local_session))
            {
                continue;
            }

            try
            {
                await local_session.SendAsync("room.state", local_payload);
            }
            catch
            {
                // Ignore transient/broken socket during broadcast so one dead client
                // does not break the whole room flow.
            }
        }
    }

    private object BuildRoomStatePayload(GameRoom room)
    {
        List<object> local_players;
        List<object> local_observers;

        lock (room.Gate)
        {
            local_players = room.PlayerSessionIds
                .Select(local_id => new
                {
                    sessionId = local_id,
                    displayName = ResolveDisplayName(local_id),
                    isReady = room.ReadyPlayerSessionIds.Contains(local_id),
                    isMj = room.MjSessionId == local_id
                })
                .Cast<object>()
                .ToList();

            local_observers = room.ObserverSessionIds
                .Select(local_id => new
                {
                    sessionId = local_id,
                    displayName = ResolveDisplayName(local_id)
                })
                .Cast<object>()
                .ToList();
        }

        return new
        {
            roomId = room.Id,
            phase = room.Phase.ToString(),
            boardSize = room.BoardSize,
            players = local_players,
            observers = local_observers
        };
    }

    private async Task BroadcastRoomEventAsync(GameRoom room, string type, object payload)
    {
        foreach (var local_sessionId in room.GetAllSessionIds())
        {
            if (!local_sessions.TryGetValue(local_sessionId, out var local_session))
            {
                continue;
            }

            await local_session.SendAsync(type, payload);
        }
    }

    private string ResolveDisplayName(string sessionId)
    {
        if (!local_sessions.TryGetValue(sessionId, out var local_session))
        {
            return sessionId;
        }

        return string.IsNullOrWhiteSpace(local_session.DisplayName) ? sessionId : local_session.DisplayName;
    }
}

public sealed class GameRoom
{
    public string Id { get; }
    public string Name { get; }
    public int BoardSize { get; }
    public object Gate { get; } = new();

    public RoomPhase Phase { get; set; } = RoomPhase.Waiting;
    public HashSet<string> PlayerSessionIds { get; } = new();
    public HashSet<string> ObserverSessionIds { get; } = new();
    public HashSet<string> ReadyPlayerSessionIds { get; } = new();

    public string? MjSessionId { get; set; }
    public int? TargetRow { get; set; }
    public int? TargetCol { get; set; }
    public DateTime? ClickStartUtc { get; set; }
    public Dictionary<string, long> ClicksMsByPlayerSessionId { get; } = new();

    public GameRoom(string id, string name, int boardSize)
    {
        Id = id;
        Name = name;
        BoardSize = boardSize;
    }

    public IEnumerable<string> GetAllSessionIds()
    {
        lock (Gate)
        {
            return PlayerSessionIds.Concat(ObserverSessionIds).ToList();
        }
    }
}

public sealed class ClientSession : IDisposable
{
    private readonly TcpClient local_tcpClient;
    private readonly NetworkStream local_stream;
    private readonly StreamReader local_reader;
    private readonly StreamWriter local_writer;
    private readonly SemaphoreSlim local_sendGate = new(1, 1);

    public string SessionId { get; } = Guid.NewGuid().ToString("N");
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? AuthToken { get; set; }
    public string? RoomId { get; set; }
    public ClientRole? Role { get; set; }
    public bool IsReady { get; set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AuthToken);

    public ClientSession(TcpClient tcpClient)
    {
        local_tcpClient = tcpClient;
        local_stream = tcpClient.GetStream();
        local_reader = new StreamReader(local_stream, Encoding.UTF8, false);
        local_writer = new StreamWriter(local_stream, new UTF8Encoding(false))
        {
            AutoFlush = true,
            NewLine = "\n"
        };
    }

    public async Task<string?> ReadLineAsync()
    {
        return await local_reader.ReadLineAsync();
    }

    public async Task SendAsync(string type, object payload)
    {
        var local_text = JsonSerializer.Serialize(new ServerEnvelope
        {
            Type = type,
            Payload = payload
        }, JsonDefaults.SerializerOptions);

        await local_sendGate.WaitAsync();
        try
        {
            await local_writer.WriteLineAsync(local_text);
        }
        finally
        {
            local_sendGate.Release();
        }
    }

    public Task SendErrorAsync(string code, string message)
    {
        return SendAsync("error", new { code, message });
    }

    public void Dispose()
    {
        local_writer.Dispose();
        local_reader.Dispose();
        local_stream.Dispose();
        local_tcpClient.Dispose();
        local_sendGate.Dispose();
    }
}

public sealed class AuthGateway
{
    private readonly HttpClient local_httpClient;

    public AuthGateway(string baseUrl)
    {
        local_httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<AuthLoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var local_response = await local_httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = password,
                RememberMe = false
            });

            if (!local_response.IsSuccessStatusCode)
            {
                return new AuthLoginResult(false, null, null, $"Web auth rejected ({(int)local_response.StatusCode}).");
            }

            var local_payload = await local_response.Content.ReadFromJsonAsync<AuthLoginWebPayload>(JsonDefaults.SerializerOptions);
            if (local_payload is null)
            {
                return new AuthLoginResult(false, null, null, "Web auth returned empty payload.");
            }

            var local_name = string.Join(' ', new[] { local_payload.FirstName, local_payload.LastName }
                .Where(local_s => !string.IsNullOrWhiteSpace(local_s)));

            return new AuthLoginResult(true, local_payload.Email, local_name, null);
        }
        catch (Exception ex)
        {
            return new AuthLoginResult(false, null, null, $"Auth gateway error: {ex.Message}");
        }
    }
}

public sealed record AuthLoginResult(bool Success, string? Email, string? DisplayName, string? Message);

public sealed class AuthLoginWebPayload
{
    public bool Success { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class ClientEnvelope
{
    public string Type { get; set; } = string.Empty;
    public JsonElement Payload { get; set; }
}

public sealed class ServerEnvelope
{
    public string Type { get; set; } = string.Empty;
    public object Payload { get; set; } = new { };
}

public sealed class LoginPayload
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class JoinRoomPayload
{
    public string RoomId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public sealed class CreateRoomPayload
{
    public string RoomName { get; set; } = string.Empty;
    public int? BoardSize { get; set; }
}

public sealed class ReadyPayload
{
    public bool Ready { get; set; }
}

public sealed class MjSelectPayload
{
    public int Row { get; set; }
    public int Col { get; set; }
}

public sealed class ClickPayload
{
    public int Row { get; set; }
    public int Col { get; set; }
}

public sealed class RoomRoundResultPayload
{
    public string? MjSessionId { get; set; }
    public string? MjDisplayName { get; set; }
    public List<RoomRoundResultEntry> Results { get; set; } = new();
}

public sealed class RoomRoundResultEntry
{
    public string SessionId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public long? ReactionMs { get; set; }
    public int? Rank { get; set; }
}

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}
