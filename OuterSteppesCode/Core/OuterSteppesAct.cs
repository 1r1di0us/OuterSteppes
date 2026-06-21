using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Unlocks;
using OuterSteppes.OuterSteppesCode.Core.Encounters;

namespace OuterSteppes.OuterSteppesCode.Core;

public sealed class OuterSteppesAct : CustomActModel
{
    public OuterSteppesAct() : base(actNumber: 2) { }

    public override IEnumerable<EncounterModel> GenerateAllEncounters() =>
    [
        ModelDb.Encounter<CrystalWalkersNormal>(),
        ModelDb.Encounter<BowlbugsWeak>(),
        ModelDb.Encounter<ChompersNormal>(),
        ModelDb.Encounter<DecimillipedeElite>(),
        ModelDb.Encounter<EntomancerElite>(),
        ModelDb.Encounter<ExoskeletonsNormal>(),
        ModelDb.Encounter<ExoskeletonsWeak>(),
        ModelDb.Encounter<HunterKillerNormal>(),
        ModelDb.Encounter<KaiserCrabBoss>(),
        ModelDb.Encounter<InfestedPrismsElite>(),
        ModelDb.Encounter<KnowledgeDemonBoss>(),
        ModelDb.Encounter<LouseProgenitorNormal>(),
        ModelDb.Encounter<MytesNormal>(),
        ModelDb.Encounter<OvicopterNormal>(),
        ModelDb.Encounter<SlumberingBeetleNormal>(),
        ModelDb.Encounter<SpinyToadNormal>(),
        ModelDb.Encounter<TheInsatiableBoss>(),
        ModelDb.Encounter<TheObscuraNormal>(),
        ModelDb.Encounter<ThievingHopperWeak>(),
        ModelDb.Encounter<HereticWeak>()
    ];

    public override IEnumerable<EventModel> AllEvents =>
    [
        ModelDb.Event<Amalgamator>(),
        ModelDb.Event<Bugslayer>(),
        ModelDb.Event<ColorfulPhilosophers>(),
        ModelDb.Event<ColossalFlower>(),
        ModelDb.Event<FieldOfManSizedHoles>(),
        ModelDb.Event<InfestedAutomaton>(),
        ModelDb.Event<LostWisp>(),
        ModelDb.Event<SpiritGrafter>(),
        ModelDb.Event<TheLanternKey>(),
        ModelDb.Event<ZenWeaver>()
    ];

    public override bool Equals(object? obj) => obj is OuterSteppesAct;
    public override int GetHashCode() => typeof(OuterSteppesAct).GetHashCode();

    // Colors differ from CustomActModel defaults (which are act 3 themed)
    public override Color MapTraveledColor => new Color("27221C");
    public override Color MapUntraveledColor => new Color("6E7750");
    public override Color MapBgColor => new Color("9B9562");

    // Original had these empty; CustomActModel defaults to act 3 music
    public override string[] BgMusicOptions => Array.Empty<string>();
    public override string[] MusicBankPaths => Array.Empty<string>();
    public override string AmbientSfx => "event:/sfx/ambience/act2_ambience";

    public override string ChestSpineResourcePath =>
        "res://animations/backgrounds/treasure_room/chest_room_act_2_skel_data.tres";

    public override string ChestSpineSkinNameNormal => "act2";
    public override string ChestSpineSkinNameStroke => "act2_stroke";
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act2";

    protected override string CustomMapTopBgPath => "res://images/packed/map/map_bgs/exordium_act/map_top_exordium_act.png";
    protected override string CustomMapMidBgPath => "res://images/packed/map/map_bgs/exordium_act/map_middle_exordium_act.png";
    protected override string CustomMapBotBgPath => "res://images/packed/map/map_bgs/exordium_act/map_middle_exordium_act.png";
    protected override string CustomRestSiteBackgroundPath => "res://scenes/rest_site/hive_rest_site.tscn";

    public override int Index => 1;
    public override bool IsDefault => false;

    public override bool IsUnlocked(UnlockState state) => true;

}