using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    Image image;

    public RectTransform rect;
    public int x;
    public int y;
    public Orb currentOrb;

    public void InitializeTile(int x, int y, Vector2 pos, float tileRadius,Sprite displaySprite)
    {
        this.x = x;
        this.y = y;
        gameObject.name = "Tile " + x + " , "+ y;
        image.sprite = displaySprite;
        rect.localPosition = pos;
        rect.sizeDelta = new Vector2(tileRadius*2, tileRadius*2);
    }
    /*
    public void OnPointerExit(PointerEventData eventData)
    {
        Board.Instance.SetCurrentTile(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Board.Instance.SetCurrentTile(this);
    }
    */
}