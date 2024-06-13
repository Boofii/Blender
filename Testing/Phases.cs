using DialoguerCore;
using System.Collections.Generic;

namespace Blender.Testing;

internal class Phase1 : AbstractDialoguePhase
{
    public Phase1(List<int> outs) : base(outs)
    {
        outs.Add(1);
    }
}

internal class Phase2 : AbstractDialoguePhase
{
    public Phase2(List<int> outs) : base(outs)
    {
        outs.Add(2);
    }
}

internal class Phase3 : AbstractDialoguePhase
{
    public Phase3(List<int> outs) : base(outs)
    {
        outs.Add(3);
    }
}