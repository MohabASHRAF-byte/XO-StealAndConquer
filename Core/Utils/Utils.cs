using Core.Models;

namespace Core.Utils;

public static class Utils
{
    public const int X = 90;

    public static CellStates CheckWinCondition(Game game)
    {
        var board = game.CellStates;

        for (var i = 0; i < 3; i++)
        {
            var rowStart = i * 3;
            if (board[rowStart] != CellStates.Empty &&
                board[rowStart] == board[rowStart + 1] &&
                board[rowStart + 1] == board[rowStart + 2])
                return board[rowStart];

            if (board[i] != CellStates.Empty &&
                board[i] == board[i + 3] &&
                board[i + 3] == board[i + 6])
                return board[i];
        }

        if (board[0] != CellStates.Empty &&
            board[0] == board[4] &&
            board[4] == board[8])
            return board[0];

        if (board[2] != CellStates.Empty &&
            board[2] == board[4] &&
            board[4] == board[6])
            return board[2];

        return CellStates.Empty;
    }
}