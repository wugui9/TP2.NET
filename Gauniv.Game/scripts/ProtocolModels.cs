using System.Collections.Generic;

namespace Gauniv.Game;

public sealed class ServerHelloPayloadModel
{
    public string? SessionId { get; set; }
    public string? Message { get; set; }
    public string? Protocol { get; set; }
}

public sealed class AuthOkPayloadModel
{
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
}

public sealed class ErrorPayloadModel
{
    public string? Code { get; set; }
    public string? Message { get; set; }
}

public sealed class RoomListPayloadModel
{
    public List<RoomSummaryModel>? Rooms { get; set; }
}

public sealed class RoomSummaryModel
{
    public string? RoomId { get; set; }
    public string? RoomName { get; set; }
    public string? Phase { get; set; }
    public int Players { get; set; }
    public int Observers { get; set; }
    public int MaxPlayers { get; set; }
    public int BoardSize { get; set; }
}

public sealed class RoomCreatedPayloadModel
{
    public string? RoomId { get; set; }
    public string? RoomName { get; set; }
    public int BoardSize { get; set; }
    public int MaxPlayers { get; set; }
}

public sealed class RoomJoinedPayloadModel
{
    public string? RoomId { get; set; }
    public string? Role { get; set; }
    public int BoardSize { get; set; }
}

public sealed class RoomStatePayloadModel
{
    public string? RoomId { get; set; }
    public string? Phase { get; set; }
    public int BoardSize { get; set; }
    public List<PlayerStateModel>? Players { get; set; }
    public List<ObserverStateModel>? Observers { get; set; }
}

public sealed class PlayerStateModel
{
    public string? SessionId { get; set; }
    public string? DisplayName { get; set; }
    public bool IsReady { get; set; }
    public bool IsMj { get; set; }
}

public sealed class ObserverStateModel
{
    public string? SessionId { get; set; }
    public string? DisplayName { get; set; }
}

public sealed class RoundStartedPayloadModel
{
    public string? MjSessionId { get; set; }
    public string? MjDisplayName { get; set; }
    public int BoardSize { get; set; }
}

public sealed class RoundTargetPayloadModel
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int ClickTimeoutSeconds { get; set; }
}

public sealed class RoundResultPayloadModel
{
    public string? MjSessionId { get; set; }
    public string? MjDisplayName { get; set; }
    public List<RoundResultEntryModel>? Results { get; set; }
}

public sealed class RoundResultEntryModel
{
    public string? SessionId { get; set; }
    public string? DisplayName { get; set; }
    public bool IsValid { get; set; }
    public long? ReactionMs { get; set; }
    public int? Rank { get; set; }
}
