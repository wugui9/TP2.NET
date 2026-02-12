using Godot;
using System.Collections.Generic;

namespace Gauniv.Game;

public partial class ResultScreen : Control
{
    [Signal]
    public delegate void BackToRoomsRequestedEventHandler();

    private Label _titleLabel = default!;
    private Label _subtitleLabel = default!;
    private ItemList _resultList = default!;
    private RoundResultPayloadModel? _pendingResult;
    private string _pendingSelfSessionId = string.Empty;
    private bool _hasPendingResult;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("Root/Stack/SummaryPanel/SummaryStack/TitleLabel");
        _subtitleLabel = GetNode<Label>("Root/Stack/SummaryPanel/SummaryStack/SubtitleLabel");
        _resultList = GetNode<ItemList>("Root/Stack/ResultPanel/ResultList");
        GetNode<Button>("Root/Stack/ActionRow/BackButton").Pressed += () => EmitSignal(SignalName.BackToRoomsRequested);

        if (_hasPendingResult)
        {
            SetRoundResult(_pendingResult, _pendingSelfSessionId);
            _hasPendingResult = false;
        }
    }

    public void SetRoundResult(RoundResultPayloadModel? result, string selfSessionId)
    {
        if (_titleLabel is null || _subtitleLabel is null || _resultList is null)
        {
            _pendingResult = result;
            _pendingSelfSessionId = selfSessionId;
            _hasPendingResult = true;
            return;
        }

        _resultList.Clear();

        if (result is null)
        {
            _titleLabel.Text = "Round Complete";
            _subtitleLabel.Text = "No result payload received.";
            return;
        }

        _titleLabel.Text = "Round Complete";
        _subtitleLabel.Text = string.IsNullOrWhiteSpace(result.MjDisplayName)
            ? "MJ: unknown"
            : $"MJ: {result.MjDisplayName}";

        var entries = result.Results ?? new List<RoundResultEntryModel>();
        foreach (var entry in entries)
        {
            var name = entry.DisplayName ?? entry.SessionId ?? "unknown";
            var rank = entry.Rank ?? 0;
            var rankText = rank switch
            {
                1 => "[1st]",
                2 => "[2nd]",
                3 => "[3rd]",
                > 0 => $"[{rank}th]",
                _ => "[--]"
            };

            var reaction = entry.ReactionMs.HasValue ? $"{entry.ReactionMs.Value} ms" : "N/A";
            var validity = entry.IsValid ? "valid" : "invalid";
            var self = string.Equals(entry.SessionId, selfSessionId) ? "  <you>" : string.Empty;

            _resultList.AddItem($"{rankText} {name}{self} | {reaction} | {validity}");
        }
    }
}
