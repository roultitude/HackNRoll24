using DG.Tweening;
using GameKit.Dependencies.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance;

    public Tile currentTile;

    public RectTransform rect;

    public RectTransform dummyRect;

    public int boardHeight, boardWidth;

    [SerializeField]
    Tile tilePrefab;

    [SerializeField]
    Orb orbPrefab;

    [SerializeField]
    TMPro.TextMeshProUGUI comboTextPrefab;

    [SerializeField]
    private Sprite[] tileSprites;

    [SerializeField]
    private AudioClip swapSound;

    private float pointMult;

    public float tileRadius;

    public Tile[,] tiles;

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
        if (currentTile == tile) return;
        if(tile != null) // try swap if currently holding
        {
            if (TouchControls.Instance.currentOrb)
            {
                Swap(TouchControls.Instance.currentOrb.currentTile, tile,true);
            }
        }
        currentTile = tile;

    }

    private void Swap(Tile from, Tile to, bool playAudio = false)
    {
        if(!GameManager.Instance.roundStarted)
        {
            GameManager.Instance.StartRound();
        }
        Orb fromOrb = from.currentOrb;
        from.currentOrb = to.currentOrb;
        to.currentOrb = fromOrb;
        if (from.currentOrb)
        {
            from.currentOrb.SetTargetLocalPos(from.rect.localPosition);
            from.currentOrb.currentTile = from;
        }
        if (to.currentOrb)
        {
            to.currentOrb.SetTargetLocalPos(to.rect.localPosition);
            to.currentOrb.currentTile = to;
        }
        if (playAudio)
        {
            AudioManager.Instance.clipSource.PlayOneShot(swapSound);
        }

        /*
        if ((from.x - to.x).Abs() <= 1 && (from.y - to.y).Abs() <= 1)
        {
            
        } else
        {
            Debug.LogError("Trying to swap non-adjacent tiles");
        }
        */

    }

    public HashSet<Tile> CheckMatches(Tile checkingTile, bool isPrimary)
    {
        HashSet<Tile> matchedOrbs = new HashSet<Tile>();

        OrbType activeOrbType = checkingTile.currentOrb.orbType;

        for (int dir = 0; dir < 4; dir++) //check for line in direction
        {
            int dist = 0;
            HashSet<Tile> line = new HashSet<Tile>();
            Tile other = IntToTileDir(dir, checkingTile);
            while (other)
            {
                if (other.currentOrb.orbType == activeOrbType)
                {
                    dist++;
                    line.Add(other);

                    if (dist > 1)
                    {
                        matchedOrbs.UnionWith(line);
                    }
                    other = IntToTileDir(dir, other);
                }
                else { break; }
            }
        }

        for (int set = 0; set < 2; set++)
        {
            Tile[] check = new Tile[] { IntToTileDir(set, checkingTile), IntToTileDir(set + 2, checkingTile) };
            foreach (Tile t in check)
            {
                if (!check[0] || !check[1]) break; // edge
                if (check[0].currentOrb.orbType == activeOrbType && check[1].currentOrb.orbType == activeOrbType)
                {
                    matchedOrbs.Add(check[0]);
                    matchedOrbs.Add(check[1]);
                }
            }
        }
        if (isPrimary)
        {
            HashSet<Tile> cache = new HashSet<Tile>(matchedOrbs);
            foreach(Tile t in cache)
            {
                matchedOrbs.UnionWith(CheckMatches(t, false));
            }
        }
        return matchedOrbs;
    }

    public IEnumerator ResolveBoard()
    {
        AudioManager.Instance.ResetPitch();
        TouchControls.Instance.isControlsActive = false;
        pointMult = 1;
        HashSet<HashSet<Tile>> pendingMatches = new HashSet<HashSet<Tile>>();
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                HashSet<Tile> matches = CheckMatches(tiles[i, j], true);
                if(matches.Count > 0)
                {
                    pendingMatches.Add(matches);
                }
                
            }
        }
        while (pendingMatches.Count > 0)
        {
            HashSet<HashSet<Tile>> cache = new HashSet<HashSet<Tile>>(pendingMatches);
            foreach (HashSet<Tile> match in cache)
            {
                pendingMatches.Remove(match);
                yield return ClearTiles(match);
            }
            ApplyGravity();
            FillPieces();
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < boardHeight; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    HashSet<Tile> matches = CheckMatches(tiles[i, j], true);
                    if (matches.Count > 0)
                    {
                        pendingMatches.Add(matches);
                    }
                }
            }

        }
        TouchControls.Instance.isControlsActive = true;
        GameManager.Instance.EndRound();
        yield return null;
    }
    

    private void ApplyGravity()
    {
        for (int j = 0; j < boardWidth; j++)
        {
            for (int i = 0; i < boardHeight; i++)
            {
                if (!tiles[i, j].currentOrb)
                {
                    for(int check = i; check < boardHeight; check++)
                    {
                        if (tiles[check, j].currentOrb)
                        {
                            Swap(tiles[i, j], tiles[check, j]);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void FillPieces()
    {
        for (int j = 0; j < boardWidth; j++)
        {
            for (int i = 0; i < boardHeight; i++)
            {
                if (!tiles[i, j].currentOrb)
                {
                    tiles[i, j].currentOrb = Instantiate(orbPrefab, rect);
                    tiles[i, j].currentOrb.Initialize(
                        new Vector2(BoardCoordToLocalPos(i, j).x , BoardCoordToLocalPos(i, j).y + boardHeight * tileRadius * 2),
                        tileRadius - 2, (OrbType)Random.Range(0, 6), tiles[i, j]);
                }
            }
        }
    }

    IEnumerator ClearTiles(HashSet<Tile> tiles)
    {
        pointMult *= 1.05f;
        for(int i = 10; i > 1; i--)
        {
            foreach(Tile tile in tiles)
            {
                if (!tile.currentOrb) { yield break; }
                tile.currentOrb.GetComponent<CanvasGroup>().alpha = (float) i / 10f;
            }
            
            yield return new WaitForSeconds(0.05f);
        }

        foreach (Tile tile in tiles)
        {
            GameManager.Instance.AddPoints(pointMult);
            TextMeshProUGUI text = Instantiate(comboTextPrefab, tile.rect.position, Quaternion.identity, dummyRect);
            text.text = pointMult.ToString("0.0") + "x";
            text.rectTransform.localScale = Vector3.one * Mathf.Sqrt(pointMult);
            text.rectTransform.DOScale(1, 1f).SetEase(Ease.InOutQuad).OnComplete(()=> Destroy(text.gameObject));
            Destroy(tile.currentOrb.gameObject);
            tile.currentOrb = null;
        }
        AudioManager.Instance.PlayMatch();
    }
    private Tile IntToTileDir(int dir, Tile tile)
    {
        switch (dir)
        {
            case 0: return tile.left;
            case 1: return tile.top;
            case 2: return tile.right;
            case 3: return tile.bot;
            default:
                return null;
        }
    }
    private Vector2 BoardCoordToLocalPos(int i, int j)
    {
        return new Vector2((1-boardWidth)* tileRadius + j * 2 * tileRadius, (1 - boardHeight) * tileRadius + tileRadius * 2 * i);
    }
}
