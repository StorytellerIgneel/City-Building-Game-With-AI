using UnityEngine;
using System;
using MyGame;

public class BuildingGhost : MonoBehaviour
{
    public void Initialize(BuildingDefinition buildingDefinition)
    {
        // Set ghost sprite
        SpriteRenderer sourceSprite = buildingDefinition.prefab.GetComponent<SpriteRenderer>();

        SpriteRenderer ghostSprite = GetComponent<SpriteRenderer>();
        if (sourceSprite != null && ghostSprite != null)
        {
            ghostSprite.sprite = sourceSprite.sprite;
            
            Color c = ghostSprite.color;
            c.a = 0.5f;
            ghostSprite.color = c;
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}