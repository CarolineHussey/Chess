using System.Collections.Generic;
using UnityEngine;
public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4, 
    Queen = 5, 
    King = 6

}

public class ChessPiece : MonoBehaviour
{
    public int team; //1 = black; 0 = white
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    private Vector3 desiredPosition; // to snap the piece to the selected tile
    private Vector3 desiredScale = Vector3.one; //for when piecesa re taken and removed from the board

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tilecountY ) //use ref here to make sure the board isn't changed, but can be accessed and used 'read only'
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //to test it works by calling on all of the slots in the middle
        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));

        return r;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }
}
