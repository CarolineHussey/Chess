using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialMove
{
    None, 
    EnPassant, 
    Castling,
    Promotion
}
public class ChessB : MonoBehaviour
{

    [Header("Artwork")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.5f;
    [SerializeField] private float deathSpacing = 0.5f;
    [SerializeField] private float dragOffset = 1.0f;
    [SerializeField] private GameObject victoryScreen;

    [Header("Materials & Prefabs")]
    [SerializeField] private GameObject[] Prefabs;
    [SerializeField] private Material[] teamMaterials;

    // LOGIC
    private ChessPiece[,] chessPieceLocation;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    

    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhiteTurn;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    

    private void Awake()
    {
        isWhiteTurn = true;
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();

    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            
            //get the indices of the tile i've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // if we are hovering a tile after not hovering any tiles:

            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //if we were already hovering a ile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //if we click mouse button
            if (Input.GetMouseButtonDown(0))
            {
                if(chessPieceLocation[hitPosition.x, hitPosition.y] != null)
                {
                    //Turn Taking
                    if((chessPieceLocation[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieceLocation[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        currentlyDragging = chessPieceLocation[hitPosition.x, hitPosition.y];

                        //highlight the available tiles
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieceLocation, TILE_COUNT_X, TILE_COUNT_Y);
                        //get a list of special moves
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPieceLocation, ref moveList, ref availableMoves);

                        PreventCheck();//NB run this before highlight tiles so that the options that may endanger the king will not light up

                        HighlightTiles();
                    }
                }
            }

            //if we release the mouse button
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY); //the position of the piece before the cursor is released
                
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));

                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            if(currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
            if(currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }



    // Generate Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {

        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;


        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);


    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X: {0}, Y: {1}", x, y));
        tileObject.transform.parent = transform;
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        tileObject.layer = LayerMask.NameToLayer("Tile");

        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Spawn Pieces

    private void SpawnAllPieces()
    {
        chessPieceLocation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        //Black Team 
        int blackTeam = 1;
        chessPieceLocation[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieceLocation[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieceLocation[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieceLocation[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieceLocation[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieceLocation[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieceLocation[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieceLocation[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieceLocation[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);


        //White Team
        int whiteTeam = 0;
        chessPieceLocation[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieceLocation[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieceLocation[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieceLocation[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieceLocation[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieceLocation[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieceLocation[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieceLocation[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieceLocation[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(Prefabs[(int)type - 1], transform).GetComponent<ChessPiece>(); //Use instantiate to spawn a chesspiece from the list of prefabs in the inspector (added -1 here because the list of prefabs in the inspector is one less than the number of ChessPieceType Enum in ChessPiece)

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[((team == 0) ? 0 : 6) + ((int)type - 1)];

        return cp;

    }

    //Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieceLocation[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieceLocation[x, y].currentX = x;
        chessPieceLocation[x, y].currentY = y;
        chessPieceLocation[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    //Highlighting Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        availableMoves.Clear();
    }

    //Checkmate
    private void Checkmate(int team)
    {
        DisplayVictoryScreen(team);
    }

    private void DisplayVictoryScreen(int winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);

    }

    public void OnResetButton()
    {
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);
        
        //remove Game Objects
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieceLocation[x, y] != null)
                    Destroy(chessPieceLocation[x, y].gameObject);

                chessPieceLocation[x, y] = null;
            }
        }

        for (int x = 0; x < deadWhites.Count; x++)
            Destroy(deadWhites[x].gameObject);

        for (int y = 0; y < deadBlacks.Count; y++)
            Destroy(deadBlacks[y].gameObject);

        //reset values
        deadWhites.Clear();
        deadBlacks.Clear();

        currentlyDragging = null;
        availableMoves.Clear();
        moveList.Clear();

        //restart board
        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    // Special Moves

    private void ProcessSpecialMove()
    {

        //En Passant
        if(specialMove == SpecialMove.EnPassant)
        {
            Debug.Log("Process Special Move");
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieceLocation[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece otherPawn = chessPieceLocation[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if(myPawn.currentX == otherPawn.currentX) //if both pawns are on the same x axis location
            {
                if(myPawn.currentY == otherPawn.currentY - 1 || myPawn.currentY == otherPawn.currentY + 1) //checks direction (team)
                {
                    if(otherPawn.team == 0)
                    {
                        deadWhites.Add(otherPawn);
                        deadWhites.Add(otherPawn);
                        otherPawn.SetScale(Vector3.one * deathSize);
                        otherPawn.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }
                    else
                    {
                        deadBlacks.Add(otherPawn);
                        deadBlacks.Add(otherPawn);
                        otherPawn.SetScale(Vector3.one * deathSize);
                        otherPawn.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.back * deathSpacing) * deadBlacks.Count);
                    }
                    chessPieceLocation[otherPawn.currentX, otherPawn.currentY] = null;
                }
            }

        }

        //Promotion
        if(specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece pawn = chessPieceLocation[lastMove[1].x, lastMove[1].y];

            if (pawn.type == ChessPieceType.Pawn)
            {
                if(pawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieceLocation[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieceLocation[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieceLocation[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                if (pawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieceLocation[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieceLocation[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieceLocation[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        //Castling
        if(specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            //Left Rook
            if (lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0) //white team
                {
                    chessPieceLocation[3, 0] = chessPieceLocation[0, 0];
                    PositionSinglePiece(3, 0);
                    chessPieceLocation[0, 0] = null;
                }
                else if (lastMove[1].y == 7) //black team
                {
                    chessPieceLocation[3, 7] = chessPieceLocation[0, 7];
                    PositionSinglePiece(3, 7);
                    chessPieceLocation[0, 7] = null;
                }
            }

            //Right Rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) //white team
                {
                    chessPieceLocation[5, 0] = chessPieceLocation[7, 0];
                    PositionSinglePiece(5, 0);
                    chessPieceLocation[7, 0] = null;
                }
                else if (lastMove[1].y == 7) //black team
                {
                    chessPieceLocation[5, 7] = chessPieceLocation[7, 7];
                    PositionSinglePiece(5, 7);
                    chessPieceLocation[7, 7] = null;
                }
            }
        }
    }

    private void PreventCheck() 
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieceLocation[x, y] != null)
                    if (chessPieceLocation[x, y].type == ChessPieceType.King)
                        if (chessPieceLocation[x, y].team == currentlyDragging.team)
                            targetKing = chessPieceLocation[x, y];
        
        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing); //currentlyDragging = the chess piece that has been picked up (and will simulate for)

    }

    private bool CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = chessPieceLocation[lastMove[1].x, lastMove[1].y].team == 0 ? 1 : 0;

        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        List<ChessPiece> attackingPieces = new List<ChessPiece>();

        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieceLocation[x, y] != null)
                {
                    if (chessPieceLocation[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieceLocation[x, y]);
                        if(chessPieceLocation[x, y].type == ChessPieceType.King)
                            targetKing = chessPieceLocation[x, y];
                    }
                    else
                    {
                        attackingPieces.Add(chessPieceLocation[x, y]);
                    }
                }
        
        //check if king is under attack
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieceLocation, TILE_COUNT_X, TILE_COUNT_Y);
            for (int k = 0; k < pieceMoves.Count; k++)
                currentAvailableMoves.Add(pieceMoves[k]);
        }

        //are we in check?
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            //if king is under threat, can we protect him?
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieceLocation, TILE_COUNT_X, TILE_COUNT_Y);

                //if we are in checkmate defendingMoves will be empty
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);

                //if we find a move that can protect the king
                if (defendingMoves.Count != 0)
                    return false; //king is in check, but not in checkmate

            }

            //if defendingMoves = 0 we cannot find any moves to defend the king;
            return true; //Checkmate exit

        }

        return false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cp">Chess piece - the one we are trying to simulate for</param>
    /// <param name="moves">a reference to available moves (this is a list of V2Int, we can delete moves that will lead to check based on results of the simulation)</param>
    /// <param name="targetKing">the King whose check we are simulating</param>
    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        //Relevant Values
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();
        
        //Simulate all moves and check if we are in check

        for (int i = 0; i < moves.Count; i++)
        {
            //create sim values for X & Y axis of availableMoves
            int simX = moves[i].x;
            int simY = moves[i].y;

            //the current location of the King
            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);

            //if currentlyDragging == King, we have to set the King's position to our simX & simY
            if (cp.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);

            //Make a COPY of the Chess Board (so that the real board is not changed!)
            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            //list of possible moves form the opposing team
            List<ChessPiece> simulateAttackingPieces = new List<ChessPiece>();

            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    //if there is a chess piece on the tile
                    if (chessPieceLocation[x, y] != null)
                    {
                        //copy it to the simulated board
                        simulation[x, y] = chessPieceLocation[x, y];
                        if (simulation[x, y].team != cp.team)
                            simulateAttackingPieces.Add(simulation[x, y]);
                    }
                }
            }

            //Simulate the move on simulated board
            simulation[actualX, actualY] = null; 
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            //did anything get taken during our simulation?
            var deadPiece = simulateAttackingPieces.Find(i => i.currentX == simX && i.currentY == simY);
            if (deadPiece != null)
                simulateAttackingPieces.Remove(deadPiece);

            //get all moves available to attacking pieces
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int j = 0; j < simulateAttackingPieces.Count; j++)
            {
                var pieceMoves = simulateAttackingPieces[j].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (int k = 0; k < pieceMoves.Count; k++)
                    simMoves.Add(pieceMoves[k]);
            }

            //Do any of those moves create a check mate scenario? if so, remove them
            if(ContainsValidMove(ref simMoves, kingPositionThisSim)) //if this returns true then the King was found in the simMoves list.
            {
                movesToRemove.Add(moves[i]);
            }

            //Restore the actual chess board so we are no longer simming
            cp.currentX = actualX;
            cp.currentY = actualY;

        }



        //Remove any options that will lead to checkmate from the availableMoves list
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);

    }

    //Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        //is the tile we are trying to move to occupied?
        if(chessPieceLocation[x, y] != null)
        {
            ChessPiece occupier = chessPieceLocation[x, y];

            if (cp.team == occupier.team)
                return false;

            //if it's occupied by the opponent
            if (occupier.team == 0)
            {
                if (occupier.type == ChessPieceType.King)
                    Checkmate(1);
                deadWhites.Add(occupier);
                occupier.SetScale(Vector3.one * deathSize);
                occupier.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * deathSpacing) * deadWhites.Count);
            }

            else
            {
                if (occupier.type == ChessPieceType.King)
                    Checkmate(0);
                deadBlacks.Add(occupier);
                occupier.SetScale(Vector3.one * deathSize);
                occupier.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }

        }

        chessPieceLocation[x, y] = cp;
        chessPieceLocation[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y)} );

        ProcessSpecialMove();
        if(CheckForCheckmate())
        {
            Checkmate(cp.team);
        }

        return true;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one; // this will indicate if tiles aren't beig generated as it will cause the game to crash :/
    }

}
