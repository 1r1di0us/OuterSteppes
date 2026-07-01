using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using OuterSteppes.OuterSteppesCode.Relics;

namespace OuterSteppes.OuterSteppesCode.Core.Events;

public class CentennialFountain : CustomEventModel
{
    public override ActModel[] Acts => new[] { ModelDb.Act<OuterSteppesAct>() };
    
    private const string _relicKey = "Relic";
    private const string _curseKey = "Curse";
    private const string _goldCostKey = "GoldCost";
    private const string _goldGainKey = "GoldGain";
    
    
    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All(p => p.Gold >= 100);
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>(new DynamicVar[4]
    {
        new GoldVar("GoldCost", 100),
        new GoldVar("GoldGain", 150),
        new StringVar("Relic", ModelDb.Relic<FairCoin>().Title.GetFormattedText()),
        new StringVar("Curse", ModelDb.Card<Guilty>().Title)
    });
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        EventOption[] array = new EventOption[2];
        Func<Task> onChosen = Choose;
        Func<Task> onPilfered = Pilfer;
        List<IHoverTip> chooseList = new List<IHoverTip>();
        chooseList.AddRange(HoverTipFactory.FromRelic<FairCoin>());
        List<IHoverTip> pilferList = new List<IHoverTip>();
        pilferList.AddRange(HoverTipFactory.FromCardWithCardHoverTips<Guilty>());
        array[0] = new EventOption(this, onChosen, "OUTERSTEPPES-CENTENNIAL_FOUNTAIN.pages.INITIAL.options.CHOOSE", chooseList.ToArray());
        array[1] = new EventOption(this, onPilfered, "OUTERSTEPPES-CENTENNIAL_FOUNTAIN.pages.INITIAL.options.PILFER", pilferList.ToArray());
        return array;
    }
    
    private async Task Choose()
    {
        await PlayerCmd.LoseGold(DynamicVars["GoldCost"].BaseValue, Owner);
        await RelicCmd.Obtain<FairCoin>(Owner);
        SetEventFinished(L10NLookup("OUTERSTEPPES-CENTENNIAL_FOUNTAIN.pages.CHOOSE.description"));
    }

    private async Task Pilfer()
    {
        await CardPileCmd.AddCursesToDeck([ModelDb.Card<Guilty>()], Owner);
        await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner);
        SetEventFinished(L10NLookup("OUTERSTEPPES-CENTENNIAL_FOUNTAIN.pages.PILFER.description"));
    }
}