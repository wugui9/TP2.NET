using Godot;
using System.Collections.Generic;
using System.Linq;

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
        var validCount = entries.Count(e => e.IsValid);
        _subtitleLabel.Text += $" | Valid: {validCount}/{entries.Count}";

        if (entries.Count == 0)
        {
            _resultList.AddItem("No ranking entries.");
            return;
        }

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
            var isSelf = string.Equals(entry.SessionId, selfSessionId);
            var self = isSelf ? "  <you>" : string.Empty;

            var index = _resultList.GetItemCount();
            _resultList.AddItem($"{rankText} {name}{self} | {reaction} | {validity}");

            var color = rank switch
            {
                1 => new Color(1.0f, 0.90f, 0.60f),
                2 => new Color(0.88f, 0.94f, 1.0f),
                3 => new Color(1.0f, 0.84f, 0.74f),
                _ => (entry.IsValid ? new Color(0.88f, 1.0f, 0.90f) : new Color(1.0f, 0.78f, 0.78f))
            };

            if (isSelf)
            {
                color = new Color(0.72f, 1.0f, 1.0f);
            }

            _resultList.SetItemCustomFgColor(index, color);
        }
    }
}
