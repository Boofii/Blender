using System;
using UnityEngine;

namespace Blender.Testing;

public class CupyInteraction : MapDialogueInteraction
{
    public override void Awake()
    {
        base.Awake();
        dialogueProperties = new AbstractUIInteractionDialogue.Properties();
        dialogueOffset = new Vector2(0F, 0.7F);
        speechBubblePosition = new Vector2(0F, 1.25F);
        speechBubblePrefab = GameObject.Find("CoinMoneyman").GetComponent<MapDialogueInteraction>().speechBubblePrefab;
        dialogueInteraction = (DialoguerDialogues)Enum.Parse(typeof(DialoguerDialogues), "Cupy_W1");
        tailOnTheLeft = true;
    }
}