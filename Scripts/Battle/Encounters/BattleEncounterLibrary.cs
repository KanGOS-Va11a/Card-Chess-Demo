using System;
using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Encounters;

[GlobalClass]
public partial class BattleEncounterLibrary : Resource
{
    [Export] public BattleEncounterProfile[] Entries { get; set; } = Array.Empty<BattleEncounterProfile>();

    public BattleEncounterProfile? FindEntry(string encounterId)
    {
        return Entries.FirstOrDefault(entry => string.Equals(entry.EncounterId, encounterId, StringComparison.Ordinal));
    }
}
