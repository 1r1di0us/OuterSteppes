using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace OuterSteppes.OuterSteppesCode.Powers;

public class HeresyPower : CustomPowerModel
{
    private bool _wasJustAppliedByEnemy;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];

    private bool WasJustAppliedByEnemy
    {
        get => _wasJustAppliedByEnemy;
        set
        {
            AssertMutable();
            _wasJustAppliedByEnemy = value;
        }
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner.IsEnemy)
            WasJustAppliedByEnemy = true;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        HeresyPower heresyPower = this;
        if (!participants.Contains(heresyPower.Owner))
            return;
        if (heresyPower.WasJustAppliedByEnemy)
        {
            heresyPower.WasJustAppliedByEnemy = false;
        }
        else
        {
            heresyPower.Flash();
            DexterityPower dexterityPower = await PowerCmd.Apply<DexterityPower>(choiceContext, heresyPower.Owner, heresyPower.Amount, heresyPower.Owner, null);
        }
    }
}