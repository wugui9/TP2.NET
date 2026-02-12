using Godot;
using System.Collections.Generic;

namespace Gauniv.Game;

public partial class GameScreen : Control
{
    [Signal]
    public delegate void CellActionRequestedEventHandler(int row, int col);

    [Signal]
    public delegate void BackToLobbyRequestedEventHandler();

    private Label _headerLabel = default!;
    private Label _phaseLabel = default!;
    private Label _targetLabel = default!;
    private Label _instructionLabel = default!;
    private GridContainer _boardGrid = default!;

    private readonly List<Button> _cellButtons = new();
    private int _boardSize;
    private int _currentTargetIndex = -1;
    private Tween? _targetPulseTween;
    private int? _pendingBoardSize;
    private PendingGameState? _pendingGameState;

    public override void _Ready()
    {
        _headerLabel = GetNode<Label>("Root/Stack/HudPanel/HudStack/HeaderLabel");
        _phaseLabel = GetNode<Label>("Root/Stack/HudPanel/HudStack/PhaseLabel");
        _targetLabel = GetNode<Label>("Root/Stack/ArenaRow/BoardPanel/BoardStack/TargetLabel");
        _instructionLabel = GetNode<Label>("Root/Stack/ArenaRow/SidePanel/SideStack/InstructionLabel");
        _boardGrid = GetNode<GridContainer>("Root/Stack/ArenaRow/BoardPanel/BoardStack/BoardGrid");

        GetNode<Button>("Root/Stack/ArenaRow/SidePanel/SideStack/BackLobbyButton").Pressed += () => EmitSignal(SignalName.BackToLobbyRequested);

        if (_pendingBoardSize.HasValue)
        {
            SetBoardSize(_pendingBoardSize.Value);
            _pendingBoardSize = null;
        }

        if (_pendingGameState is not null)
        {
            var state = _pendingGameState;
            _pendingGameState = null;
            SetGameState(state.RoomId, state.Phase, state.Role, state.IsMj, state.Instruction, state.TargetRow, state.TargetCol, state.CanAct);
        }
    }

    public void SetBoardSize(int boardSize)
    {
        if (_boardGrid is null)
        {
            _pendingBoardSize = boardSize;
            return;
        }

        if (boardSize <= 0)
        {
            boardSize = 5;
        }

        if (_boardSize == boardSize && _cellButtons.Count == boardSize * boardSize)
        {
            return;
        }

        _boardSize = boardSize;
        ClearTargetPulse();

        foreach (var btn in _cellButtons)
        {
            btn.QueueFree();
        }
        _cellButtons.Clear();

        _boardGrid.Columns = _boardSize;

        for (var row = 0; row < _boardSize; row++)
        {
            for (var col = 0; col < _boardSize; col++)
            {
                var button = new Button
                {
                    Text = " ",
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                    SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                    CustomMinimumSize = new Vector2(76, 76),
                    TooltipText = $"Cell ({row},{col})"
                };

                var localRow = row;
                var localCol = col;
                button.Pressed += () => EmitSignal(SignalName.CellActionRequested, localRow, localCol);

                _boardGrid.AddChild(button);
                _cellButtons.Add(button);
            }
        }
    }

    public void SetGameState(string roomId, string phase, string role, bool isMj, string instruction, int? targetRow, int? targetCol, bool canAct)
    {
        if (_headerLabel is null || _phaseLabel is null || _targetLabel is null || _instructionLabel is null)
        {
            _pendingGameState = new PendingGameState(roomId, phase, role, isMj, instruction, targetRow, targetCol, canAct);
            return;
        }

        _headerLabel.Text = $"ROOM {roomId} | ROLE {role}" + (isMj ? " | MJ" : string.Empty);
        _phaseLabel.Text = $"PHASE: {phase}";

        if (targetRow.HasValue && targetCol.HasValue)
        {
            _targetLabel.Text = $"TARGET CELL: ({targetRow.Value},{targetCol.Value})";
        }
        else
        {
            _targetLabel.Text = "TARGET CELL: pending";
        }

        _instructionLabel.Text = canAct
            ? $"ACTION: {instruction}"
            : $"WATCH: {instruction}";
        _instructionLabel.SelfModulate = canAct
            ? new Color(0.82f, 1.0f, 0.84f)
            : new Color(0.92f, 0.94f, 1.0f);

        ClearTargetPulse();

        for (var i = 0; i < _cellButtons.Count; i++)
        {
            var button = _cellButtons[i];
            button.Disabled = !canAct;
            button.Modulate = canAct
                ? new Color(0.86f, 0.97f, 1.0f)
                : new Color(0.64f, 0.72f, 0.82f);
            button.Scale = Vector2.One;
        }

        if (targetRow.HasValue && targetCol.HasValue && _boardSize > 0)
        {
            var index = targetRow.Value * _boardSize + targetCol.Value;
            if (index >= 0 && index < _cellButtons.Count)
            {
                _currentTargetIndex = index;
                var targetColor = canAct
                    ? new Color(0.70f, 1.0f, 0.80f)
                    : new Color(0.78f, 0.90f, 1.0f);
                _cellButtons[index].Modulate = targetColor;
                StartTargetPulse(index, targetColor);
            }
        }
    }

    private void StartTargetPulse(int index, Color baseColor)
    {
        if (index < 0 || index >= _cellButtons.Count)
        {
            return;
        }

        var pulseColor = new Color(
            Mathf.Clamp(baseColor.R + 0.14f, 0, 1),
            Mathf.Clamp(baseColor.G + 0.10f, 0, 1),
            Mathf.Clamp(baseColor.B + 0.10f, 0, 1),
            1f);

        _targetPulseTween?.Kill();
        _targetPulseTween = CreateTween();
        _targetPulseTween.SetLoops();
        _targetPulseTween.TweenProperty(_cellButtons[index], "modulate", pulseColor, 0.28f);
        _targetPulseTween.TweenProperty(_cellButtons[index], "modulate", baseColor, 0.28f);
    }

    private void ClearTargetPulse()
    {
        _targetPulseTween?.Kill();
        _targetPulseTween = null;

        if (_currentTargetIndex >= 0 && _currentTargetIndex < _cellButtons.Count)
        {
            _cellButtons[_currentTargetIndex].Scale = Vector2.One;
        }

        _currentTargetIndex = -1;
    }

    private sealed record PendingGameState(
        string RoomId,
        string Phase,
        string Role,
        bool IsMj,
        string Instruction,
        int? TargetRow,
        int? TargetCol,
        bool CanAct);
}
