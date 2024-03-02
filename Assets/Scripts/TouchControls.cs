using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TouchControls : MonoBehaviour
{
    public bool isControlsActive = false;
    public static TouchControls Instance;
    public Orb currentOrb;
    [SerializeField]
    Orb phantomOrb;
    public RectTransform phantomOrbRect;

    [SerializeField]
    Vector2 holdOffset;

    [SerializeField]
    private GraphicRaycaster gr;

    private Vector3 clampedMousePos;

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
        phantomOrbRect = phantomOrb.GetComponent<RectTransform>();
        isControlsActive = false;
    }
    void Update()
    {
        if(!isControlsActive) { return; }
        clampedMousePos = Input.mousePosition;
        
        clampedMousePos.x = Mathf.Clamp(
            clampedMousePos.x,
            Board.Instance.rect.position.x - Board.Instance.tileRadius * Board.Instance.boardWidth,
            Board.Instance.rect.position.x + Board.Instance.tileRadius * Board.Instance.boardWidth);
        clampedMousePos.y = Mathf.Clamp(
            clampedMousePos.y, 
            Board.Instance.rect.position.y - Board.Instance.tileRadius * Board.Instance.boardHeight,
            Board.Instance.rect.position.y + Board.Instance.tileRadius * Board.Instance.boardHeight
            );
        bool isMouseOutOfBoard = clampedMousePos.x != Input.mousePosition.x || clampedMousePos.y != Input.mousePosition.y;
        PointerEventData ped = new PointerEventData(null);
        Debug.Log(Board.Instance.rect.position + " " + Board.Instance.tileRadius * Board.Instance.boardWidth);
        ped.position = clampedMousePos;

        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(ped, results);
        
        if (results.Count == 0)
        {
            return;
        }
        foreach(RaycastResult r in results)
        {
            
            Tile selectedTile = r.gameObject.GetComponent<Tile>();
            if (selectedTile)
            {
                Board.Instance.SetCurrentTile(selectedTile);
                break;
            }
            else
            {
                Board.Instance.SetCurrentTile(null);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Board.Instance.currentTile && !isMouseOutOfBoard)
            {
                currentOrb = Board.Instance.currentTile.currentOrb;
                currentOrb.GetComponent<CanvasGroup>().alpha = 0.5f;
                phantomOrb.Initialize(currentOrb.rect.localPosition, Board.Instance.tileRadius, currentOrb.orbType, null);
                phantomOrb.gameObject.SetActive(true);
                
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentOrb)
            {
                phantomOrb.gameObject.SetActive(false);
                currentOrb.GetComponent<CanvasGroup>().alpha = 1f;
                if (!Board.Instance.currentTile)
                {
                    currentOrb.ResetTargetLocalPos();
                    currentOrb = null;
                    return;
                }
                currentOrb.SetTargetLocalPos(Board.Instance.currentTile.rect.localPosition);
                // set currentorb targetloc
                currentOrb = null;
                Board.Instance.StartCoroutine(Board.Instance.ResolveBoard());
            }
        }
        if(currentOrb)
        {
            //currentOrb.SetTargetLocalPos(null);

            Vector3 transformedPos = Input.mousePosition;
            
            transformedPos.z = 0;
            phantomOrbRect.position = Vector2.Lerp(phantomOrbRect.position, transformedPos, Time.deltaTime * 40f);
        }
    }

    public void ForceDropOrb()
    {
        if (currentOrb)
        {
            phantomOrb.gameObject.SetActive(false);
            currentOrb.GetComponent<CanvasGroup>().alpha = 1f;
            if (!Board.Instance.currentTile)
            {
                currentOrb.ResetTargetLocalPos();
                currentOrb = null;
                return;
            }
            currentOrb.SetTargetLocalPos(Board.Instance.currentTile.rect.localPosition);
            // set currentorb targetloc
            currentOrb = null;
            Board.Instance.StartCoroutine(Board.Instance.ResolveBoard());
        }
    }
}
