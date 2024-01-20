using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VFavorites.Libs;

public class Board : MonoBehaviour
{
    public static Board Instance;

    public Tile currentTile;

    public RectTransform rect;

    public int boardHeight, boardWidth;

    [SerializeField]
    Tile tilePrefab;

    [SerializeField]
    Orb orbPrefab;



    [SerializeField]
    private Sprite[] tileSprites;
   
    public float tileRadius;

    private Tile[,] tiles;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        rect = GetComponent<RectTransform>();
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        tiles = new Tile[boardHeight,boardWidth];
        int tileSpriteCounter = 0;
        tileRadius = 500/boardWidth;
        
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                tiles[i, j] = Instantiate(tilePrefab, rect);
                tiles[i, j].InitializeTile(j,i,
                    BoardCoordToLocalPos(i,j),
                    tileRadius, 
                    tileSprites[tileSpriteCounter]);
                tileSpriteCounter = (tileSpriteCounter + 1) % tileSprites.Length;
            }
            if(boardWidth%2 == 0) tileSpriteCounter = (tileSpriteCounter + 1) % tileSprites.Length;
        }

        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                tiles[i, j].currentOrb = Instantiate(orbPrefab,rect);
                tiles[i, j].currentOrb.Initialize(BoardCoordToLocalPos(i,j), tileRadius - 2, (OrbType) Random.Range(0,6), tiles[i,j]);
            }
        }
    }

    public void SetCurrentTile(Tile tile)
    {
        if(tile != null) // try swap if currently holding
        {
            if (TouchControls.Instance.currentOrb)
            {
                Swap(TouchControls.Instance.currentOrb.currentTile, tile);
            }
        }
        currentTile = tile;

    }

    private void Swap(Tile from, Tile to)
    {
        if((from.x - to.x).Abs() <= 1 && (from.y - to.y).Abs() <= 1)
        {
            Orb fromOrb = from.currentOrb;
            from.currentOrb = to.currentOrb;
            to.currentOrb = fromOrb;
            from.currentOrb.SetTargetLocalPos(from.rect.localPosition);
            to.currentOrb.SetTargetLocalPos(to.rect.localPosition);
            from.currentOrb.currentTile = from;
            to.currentOrb.currentTile = to;
        } else
        {
            Debug.LogError("Trying to swap non-adjacent tiles");
        }

    }

    private Vector2 BoardCoordToLocalPos(int i, int j)
    {
        return new Vector2((1-boardWidth)* tileRadius + j * 2 * tileRadius, (1 - boardHeight) * tileRadius + tileRadius * 2 * i);
    }
}
