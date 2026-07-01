using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace OuterSteppes.OuterSteppesCode.Relics;

[Pool(typeof(EventRelicPool))]
public class FairCoin : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3M, ValueProp.Unpowered), new DamageVar(3M, ValueProp.Unpowered)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        FairCoin fairCoin = this;
        if (!participants.Contains(fairCoin.Owner.Creature))
            return;
        fairCoin.Flash();
        bool option = fairCoin.Owner.RunState.Rng.Niche.NextBool();
        if (option)
        {
            Decimal num = await CreatureCmd.GainBlock(fairCoin.Owner.Creature, fairCoin.DynamicVars.Block, null);
        }
        else
        {
            IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, fairCoin.Owner.Creature.CombatState.HittableEnemies, fairCoin.DynamicVars.Damage, fairCoin.Owner.Creature);
        }
    }
}