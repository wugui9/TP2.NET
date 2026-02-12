using Godot;
using System.Collections.Generic;

namespace Gauniv.Game;

public partial class ResultScreen : Control
{
    [Signal]
    public delegate void BackToRoomsRequestedEventHandler();

    private Label _titleLabel = default!;
    private ItemList _resultList = default!;
    private RoundResultPayloadModel? _pendingResult;
    private string _pendingSelfSessionId = string.Empty;
    private bool _hasPendingResult;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("Root/Stack/TitleLabel");
        _resultList = GetNode<ItemList>("Root/Stack/ResultList");
        GetNode<Button>("Root/Stack/BackButton").Pressed += () => EmitSignal(SignalName.BackToRoomsRequested);

        if (_hasPendingResult)
        {
            SetRoundResult(_pendingResult, _pendingSelfSessionId);
            _hasPendingResult = false;
        }
    }

    public void SetRoundResult(RoundResultPayloadModel? result, string selfSessionId)
    {
        if (_titleLabel is null || _resultList is null)
        {
            _pendingResult = result;
            _pendingSelfSessionId = selfSessionId;
            _hasPendingResult = true;
            return;
        }

        _resultList.Clear();

        if (result is null)
        {
            _titleLabel.Text = "Round Result";
            return;
        }

        _titleLabel.Text = "Round Result";
        if (!string.IsNullOrWhiteSpace(result.MjDisplayName))
        {
            _titleLabel.Text += $" | MJ: {result.MjDisplayName}";
        }

        var entries = result.Results ?? new List<RoundResultEntryModel>();
        foreach (var entry in entries)
        {
            var name = entry.DisplayName ?? entry.SessionId ?? "unknown";
            var rank = entry.Rank.HasValue ? entry.Rank.Value.ToString() : "-";
            var reaction = entry.ReactionMs.HasValue ? $"{entry.ReactionMs.Value}ms" : "N/A";
            var valid = entry.IsValid ? "valid" : "invalid";
            var self = string.Equals(entry.SessionId, selfSessionId) ? " (you)" : string.Empty;
            _resultList.AddItem($"#{rank} {name}{self} | {reaction} | {valid}");
        }
    }
}
