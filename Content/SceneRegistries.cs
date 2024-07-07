using Blender.Utility;
using DialoguerCore;
using HarmonyLib;
using System;

namespace Blender.Content;

public class SceneRegistries
{
    public static readonly LinkedRegistry<DialoguerDialogues, DialoguerDialogue> Dialogues = new(
        (name, dialogue) =>
        {
            foreach (AbstractDialoguePhase phase in dialogue.phases)
            {
                if (phase is TextPhase textPhase)
                {
                    Traverse<int> traverse = Traverse.Create(textPhase.data).Field<int>("dialogueID");
                    traverse.Value = (int)Enum.Parse(typeof(DialoguerDialogues), name);
                }
            }
        });
}