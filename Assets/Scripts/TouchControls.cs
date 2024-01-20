using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchControls : MonoBehaviour
{

    public static TouchControls Instance;
    public Orb currentOrb;
    [SerializeField]
    Orb phantomOrb;
    public RectTransform phantomOrbRect;

    [SerializeField]
    Vector2 holdOffset;


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
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Board.Instance.currentTile)
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
            }
        }
        if(currentOrb)
        {
            //currentOrb.SetTargetLocalPos(null);

            Vector3 transformedPos = Input.mousePosition;
            
            transformedPos.z = 0;
            Debug.Log(transformedPos);
            phantomOrbRect.position = Vector2.Lerp(phantomOrbRect.position, transformedPos, Time.deltaTime * 40f);
        }
    }
}
