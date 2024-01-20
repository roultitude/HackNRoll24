using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OrbType { whimsy, discovery, imaginary, reality, creativity, recovery, monotony, toxicity }
public class Orb : MonoBehaviour
{
    [SerializeField]
    Sprite[] orbSprites;

    [SerializeField]
    Image image;

    [SerializeField]
    float lerpSpeed;

    public RectTransform rect;
    public OrbType orbType;
    private Vector3? targetLocalPos;
    public Tile currentTile;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Initialize(Vector2 spawnPos, float orbRadius, OrbType type, Tile currentTile)
    {
        orbType = type;
        image.sprite = orbSprites[(int) type];
        rect.sizeDelta = new Vector2(orbRadius * 2, orbRadius * 2);
        rect.localPosition = spawnPos;
        this.currentTile = currentTile;
    }

    private void Update()
    {
        if (!targetLocalPos.HasValue) return;

        rect.localPosition = Vector3.Lerp(rect.localPosition, (Vector3)targetLocalPos, Time.deltaTime * lerpSpeed);
    }

    public void SetTargetLocalPos(Vector3? pos)
    {
        targetLocalPos = pos;
    }
    public void ResetTargetLocalPos()
    {
        targetLocalPos = currentTile.GetComponent<RectTransform>().localPosition;
    }

}
