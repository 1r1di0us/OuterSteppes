using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using OuterSteppes.OuterSteppesCode.Powers;

namespace OuterSteppes.OuterSteppesCode.Core.Monsters;

public class Heretic : CustomMonsterModel
{
  private static readonly LocString _heresyDialogue = new LocString("monsters", "OUTERSTEPPES-HERETIC.moves.HERESY_MOVE.banter");
  private const string BuffSfx = "event:/sfx/enemy/enemy_attacks/cultists/cultists_buff_calcified";
  private float _attackSfxStrength;

  public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 100, 97);

  public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 108, 104);

  private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);
  
  private int BlockGain => 10;
  
  private int BigAttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 30, 26);

  private int HeresyAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 6, 5);

  public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

  public override Vector2 ExtraDeathVfxPadding => new Vector2(1.5f, 1.2f);

  public override string DeathSfx
  {
    get => "event:/sfx/enemy/enemy_attacks/cultists/cultists_die_calcified";
  }

  protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/cultists/cultists_attack";

  private float AttackSfxStrength
  {
    get => _attackSfxStrength;
    set
    {
      AssertMutable();
      _attackSfxStrength = value;
    }
  }

  public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
  {
    MegaSkin skin = spine.NewSkin("custom-skin");
    MegaSkeletonDataResource data = skeleton.GetData();
    skin.AddSkin(data.FindSkin("coral"));
    skeleton.SetSkin(skin);
    skeleton.SetSlotsToSetupPose();
  }

  protected override MonsterMoveStateMachine GenerateMoveStateMachine()
  {
    List<MonsterState> states = new List<MonsterState>();
    MoveState initialState = new MoveState("HERESY_MOVE", HeresyMove, new BuffIntent());
    MoveState moveState1 = new MoveState("ATTACK_BLOCK_MOVE_1", AttackBlockMove, new SingleAttackIntent(AttackDamage), new DefendIntent());
    MoveState moveState2 = new MoveState("ATTACK_BLOCK_MOVE_2", AttackBlockMove, new SingleAttackIntent(AttackDamage), new DefendIntent());
    MoveState moveState3 = new MoveState("ATTACK_BLOCK_MOVE_3", AttackBlockMove, new SingleAttackIntent(AttackDamage), new DefendIntent());
    MoveState moveState4 = new MoveState("BIG_ATTACK_MOVE", BigAttackMove, new SingleAttackIntent(BigAttackDamage));
    initialState.FollowUpState = moveState1;
    moveState1.FollowUpState = moveState2;
    moveState2.FollowUpState = moveState3;
    moveState3.FollowUpState = moveState4;
    moveState4.FollowUpState = moveState1;
    states.Add(initialState);
    states.Add(moveState1);
    states.Add(moveState2);
    states.Add(moveState3);
    states.Add(moveState4);
    return new MonsterMoveStateMachine(states, initialState);
  }

  private async Task HeresyMove(IReadOnlyList<Creature> targets)
  {
    Heretic heretic = this;
    SfxCmd.Play(BuffSfx);
    await CreatureCmd.TriggerAnim(heretic.Creature, "Cast", 0.5f);
    TalkCmd.Play(Heretic._heresyDialogue, heretic.Creature, VfxColor.Purple, VfxDuration.Standard);
    await Cmd.CustomScaledWait(0.25f, 0.5f);
    HeresyPower heresyPower = await PowerCmd.Apply<HeresyPower>(new ThrowingPlayerChoiceContext(), heretic.Creature, heretic.HeresyAmount, heretic.Creature, null);
  }

  private async Task AttackBlockMove(IReadOnlyList<Creature> targets)
  {
    Heretic heretic = this;
    AttackCommand attackCommand = await DamageCmd.Attack(heretic.AttackDamage).FromMonster(heretic).WithAttackerAnim("Attack", 0.2f).BeforeDamage(heretic.PlayAttackSfx).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    Decimal num = await CreatureCmd.GainBlock(heretic.Creature, heretic.BlockGain, ValueProp.Move, null);
  }

  private async Task BigAttackMove(IReadOnlyList<Creature> targets)
  {
    Heretic monster = this;
    AttackCommand attackCommand = await DamageCmd.Attack(monster.BigAttackDamage).FromMonster(monster).WithAttackerAnim("Attack", 0.2f).BeforeDamage(monster.PlayAttackSfx).WithHitFx("vfx/vfx_attack_slash").Execute(null);
  }
  
  public override CreatureAnimator GenerateAnimator(MegaSprite controller)
  {
    AnimState initialState = new AnimState("idle_loop", true);
    AnimState state1 = new AnimState("buff");
    AnimState state2 = new AnimState("attack");
    AnimState state3 = new AnimState("hurt");
    AnimState state4 = new AnimState("die");
    state1.NextState = initialState;
    state2.NextState = initialState;
    state3.NextState = initialState;
    CreatureAnimator animator = new CreatureAnimator(initialState, controller);
    animator.AddAnyState("Cast", state1);
    animator.AddAnyState("Attack", state2);
    animator.AddAnyState("Dead", state4);
    animator.AddAnyState("Hit", state3);
    return animator;
  }

  private Task PlayAttackSfx()
  {
    SfxCmd.Play(AttackSfx, "enemy_strength", AttackSfxStrength);
    AttackSfxStrength += 0.2f;
    return Task.CompletedTask;
  }
}