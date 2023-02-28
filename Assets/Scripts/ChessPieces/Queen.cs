
using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Down

        for (int i = currentY - 1; i >= 0; i--)
        {
            if (board[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i)); //if null, add to list & keep going

            if (board[currentX, i] != null) //when it's not null, there is a piece between my piece and the direction of trvel.The rook cannot go over pieces, so we don't need ot keep going - add break at end of statement
            {
                if (board[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX, i));

                break; //the piece can only move this far!
            }

        }

        // Up

        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (board[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i)); //if null, add to list & keep going

            if (board[currentX, i] != null) //when it's not null, there is a piece between my piece and the direction of trvel.The rook cannot go over pieces, so we don't need ot keep going - add break at end of statement
            {
                if (board[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX, i));

                break; //the piece can only move this far!
            }

        }

        // Left

        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY)); //if null, add to list & keep going

            if (board[i, currentY] != null) //when it's not null, there is a piece between my piece and the direction of trvel.The rook cannot go over pieces, so we don't need ot keep going - add break at end of statement
            {
                if (board[i, currentY].team != team)
                    r.Add(new Vector2Int(i, currentY));

                break; //the piece can only move this far!
            }

        }


        // Right

        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (board[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY)); //if null, add to list & keep going

            if (board[i, currentY] != null) //when it's not null, there is a piece between my piece and the direction of trvel.The rook cannot go over pieces, so we don't need ot keep going - add break at end of statement
            {
                if (board[i, currentY].team != team)
                    r.Add(new Vector2Int(i, currentY));

                break; //the piece can only move this far!
            }

        }

        //top right
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }

        //top left
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }

        //bottom right
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }

        //bottom left
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }
        return r;
    }
    }
