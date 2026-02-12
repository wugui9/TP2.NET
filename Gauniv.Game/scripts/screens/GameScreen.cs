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
    private int _boardSize = 0;
    private int? _pendingBoardSize;
    private PendingGameState? _pendingGameState;

    public override void _Ready()
    {
        _headerLabel = GetNode<Label>("Root/Stack/HeaderLabel");
        _phaseLabel = GetNode<Label>("Root/Stack/PhaseLabel");
        _targetLabel = GetNode<Label>("Root/Stack/TargetLabel");
        _instructionLabel = GetNode<Label>("Root/Stack/InstructionLabel");
        _boardGrid = GetNode<GridContainer>("Root/Stack/BoardGrid");

        GetNode<Button>("Root/Stack/ActionRow/BackLobbyButton").Pressed += () => EmitSignal(SignalName.BackToLobbyRequested);

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
                    Text = $"{row},{col}",
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                    SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                    CustomMinimumSize = new Vector2(52, 52)
                };

                var localRow = row;
                var localCol = col;
                button.Pressed += () => EmitSignal(SignalName.CellActionRequested, localRow, localCol);

                _boardGrid.AddChild(button);
                _cellButtons.Add(button);
            }
        }
    }

    public void SetGameState(string roomId, string phase, string role, bool isMj, string instruction, int? targetRow, int?targetCol, bool canAct)
    {
        if (_headerLabel is null || _phaseLabel is null || _targetLabel is null || _instructionLabel is null)
        {
            _pendingGameState = new PendingGameState(roomId, phase, role, isMj, instruction, targetRow, targetCol, canAct);
            return;
        }

        _headerLabel.Text = $"Room: {roomId} | Role: {role}" + (isMj ? " | You are MJ" : string.Empty);
        _phaseLabel.Text = $"Phase: {phase}";
        _instructionLabel.Text = instruction;

        if (targetRow.HasValue && targetCol.HasValue)
        {
            _targetLabel.Text = $"Target: ({targetRow.Value},{targetCol.Value})";
        }
        else
        {
            _targetLabel.Text = "Target: not selected";
        }

        foreach (var btn in _cellButtons)
        {
            btn.Disabled = !canAct;
        }
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
