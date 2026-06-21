using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;

namespace OuterSteppes.OuterSteppesCode.Core.Encounters;

public sealed class HereticWeak : EncounterModel
{
  private static readonly Dictionary<MonsterModel, int> WorkerValidCounts = new ()
  {
    {
      ModelDb.Monster<BowlbugEgg>(),
      1
    },
    {
      ModelDb.Monster<BowlbugSilk>(),
      1
    },
    {
      ModelDb.Monster<BowlbugNectar>(),
      1
    }
  };

  private static readonly string[] SlotNames = ["first", "middle", "last"];

  public override IEnumerable<EncounterTag> Tags => [(EncounterTag.Workers)];

  public override RoomType RoomType => RoomType.Monster;

  public override bool HasScene => true;

  public override IEnumerable<MonsterModel> AllPossibleMonsters
  {
    get
    {
      Dictionary<MonsterModel, int>.KeyCollection keys = WorkerValidCounts.Keys;
      int index = 0;
      MonsterModel[] items = new MonsterModel[1 + keys.Count];
      foreach (MonsterModel monsterModel in keys)
      {
        items[index] = monsterModel;
        ++index;
      }
      items[index] = ModelDb.Monster<BowlbugRock>();
      return items;
    }
  }

  protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
  {
    List<MonsterModel> currentWorkers = new List<MonsterModel>();
    int num = 1;
    List<(MonsterModel, string)> list = new List<(MonsterModel, string)>(num);
    CollectionsMarshal.SetCount(list, num);
    CollectionsMarshal.AsSpan(list)[0] = (ModelDb.Monster<BowlbugRock>().ToMutable(), SlotNames[0]);
    List<(MonsterModel, string)> monsters = list;
    for (int index = 0; index < 2; ++index)
    {
      MonsterModel monsterModel = Rng.NextItem( WorkerValidCounts.Keys.Where(r => currentWorkers.Count(c => c == r) < WorkerValidCounts[r]).ToList());
      currentWorkers.Add(monsterModel);
      monsters.Add((monsterModel.ToMutable(), SlotNames[index + 1]));
    }
    return monsters;
  }
}