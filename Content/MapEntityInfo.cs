using System;
using UnityEngine;

namespace Blender.Content;

public class MapEntityInfo(string bundlePath, string[] scenes, Vector3 position)
{
    public string BundlePath { get; private set; } = bundlePath;
    public string[] Scenes { get; private set; } = scenes;
    public Vector3 Position { get; private set; } = position;
    public DialoguerDialogues? DialoguerDialogues { get; private set; } = null;
    public Quaternion Rotation { get; private set; } = Quaternion.identity;
    public AbstractUIInteractionDialogue.Properties DialogueProperties { get; private set; } = new();
    public Vector2 DialogueOffset = new Vector2(0, 0.7F);
    public float InteractionDistance = 1F;
    public Vector2 SpeechBubbleOffset = Vector2.zero;
    public Action<GameObject> SetupAction { get; private set; }

    public MapEntityInfo SetDialogue(DialoguerDialogues dialogue)
    {
        DialoguerDialogues = dialogue;
        return this;
    }

    public MapEntityInfo SetRotation(Quaternion rotation)
    {
        Rotation = rotation;
        return this;
    }

    public MapEntityInfo SetDialogueProperties(AbstractUIInteractionDialogue.Properties properties)
    {
        DialogueProperties = properties;
        return this;
    }

    public MapEntityInfo SetDialogueOffset(Vector2 dialogueOffset)
    {
        DialogueOffset = dialogueOffset;
        return this;
    }

    public MapEntityInfo SetInteractionDistance(float distance)
    {
        InteractionDistance = distance;
        return this;
    }

    public MapEntityInfo SetSpeechBubbleOffset(Vector2 offset)
    {
        SpeechBubbleOffset = offset;
        return this;
    }

    public MapEntityInfo SetSetupAction(Action<GameObject> setupAction) {
        SetupAction += setupAction;
        return this;
    }
}