
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1; //move forwards or backwards depending on the team

        //Move one in front
        if (board[currentX, currentY + direction] == null)
            r.Add(new Vector2Int(currentX, currentY + direction));

        //Move two in front
        if (board[currentX, currentY + direction] == null)
        {
            if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));

            if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
        }

        //Kill Move
        if (currentX != tileCountX - 1)
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
                r.Add(new Vector2Int(currentX + 1, currentY + direction));

        if (currentX != 0)
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                r.Add(new Vector2Int(currentX - 1, currentY + direction));

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;
        //En Passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if(board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn) // if last move was a pawn
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) // if that pawn moved 2 squares (Mathf.Abs allows us to check both teams (if it is a black pawn, the -2 will be changed to 2 so the boolean checks out.)
                {
                    if (board[lastMove[1].x, lastMove[1].y].team != team) //if the last move was the other team
                    {
                        if(lastMove[1].y == currentY) //if pawns from both teams are on the same y axis location
                        {
                            if(lastMove[1].x == currentX - 1) //if the pawn is 1 square to the left (x axis location -1)
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction)); //add the move to the list of available moves
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == currentX + 1) //if the pawn is 1 square to the right (x axis location +1)
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction)); //add the move to the list of available moves
                                return SpecialMove.EnPassant;
                            }

                        }
                    }
                }
            }
        }
        return SpecialMove.None;
    }
}
