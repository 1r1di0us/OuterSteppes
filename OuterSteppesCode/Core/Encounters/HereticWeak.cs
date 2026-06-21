using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using OuterSteppes.OuterSteppesCode.Core.Monsters;

namespace OuterSteppes.OuterSteppesCode.Core.Encounters;

public sealed class HereticWeak : EncounterModel
{
  public override IEnumerable<EncounterTag> Tags => [EncounterTag.Burrower];

  public override RoomType RoomType => RoomType.Monster;

  public override bool IsWeak => true;

  public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Heretic>()];

  protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
  {
    // ISSUE: object of a compiler-generated type is created
    return [(ModelDb.Monster<Heretic>().ToMutable(), null)];
  }
}