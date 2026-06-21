using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace OuterSteppes.OuterSteppesCode.Core.Monsters;

public class Heretic : MonsterModel
{
  private const string _dizzyMoveId = "DIZZY_MOVE";
  public const string biteMoveId = "BITE_MOVE";
  private const string _burrowedAttackTrigger = "BurrowAttack";
  private const string _burrowTrigger = "Burrow";
  private const string _stunTrigger = "Stun";
  private const string _wakeUpTrigger = "WakeUp";
  private const string _burrowSfx = "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_burrow";
  private const string _hiddenBurrowAttackSfx = "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hidden_attack";
  private bool _isStunned;

  public override int MinInitialHp
  {
    get => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 92, 87);
  }

  public override int MaxInitialHp => MinInitialHp;

  protected override string AttackSfx
  {
    get => "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_attack";
  }

  public override string HurtSfx
  {
    get => "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hurt";
  }

  public override string DeathSfx
  {
    get => "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_die";
  }

  private int BiteDamage
  {
    get => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);
  }

  private int BlockGain
  {
    get => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 37, 32);
  }

  private int BelowDamage
  {
    get => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 23);
  }

  public bool IsStunned
  {
    get => _isStunned;
    set
    {
      AssertMutable();
      _isStunned = value;
    }
  }

  protected override MonsterMoveStateMachine GenerateMoveStateMachine()
  {
    List<MonsterState> states = new List<MonsterState>();
    MoveState initialState = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
    MoveState moveState1 = new MoveState("BURROW_MOVE", BurrowMove, new BuffIntent(), new DefendIntent());
    MoveState moveState2 = new MoveState("BELOW_MOVE", BelowMove, new SingleAttackIntent(BelowDamage));
    MoveState moveState3 = new MoveState("DIZZY_MOVE", StillDizzyMove, new StunIntent());
    initialState.FollowUpState = moveState1;
    moveState1.FollowUpState = moveState2;
    moveState2.FollowUpState = moveState2;
    moveState3.FollowUpState = initialState;
    states.Add(initialState);
    states.Add(moveState1);
    states.Add(moveState2);
    states.Add(moveState3);
    return new MonsterMoveStateMachine(states, initialState);
  }

  private async Task BiteMove(IReadOnlyList<Creature> targets)
  {
    Heretic monster = this;
    AttackCommand attackCommand = await DamageCmd.Attack(monster.BiteDamage).FromMonster(monster).WithAttackerAnim("Attack", 0.25f).WithAttackerFx(sfx: monster.AttackSfx).WithHitFx("vfx/vfx_attack_slash").Execute(null);
  }

  private async Task BurrowMove(IReadOnlyList<Creature> targets)
  {
    Heretic heretic = this;
    SfxCmd.Play("event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_burrow");
    await CreatureCmd.TriggerAnim(heretic.Creature, "Burrow", 0.25f);
    BurrowedPower burrowedPower = await PowerCmd.Apply<BurrowedPower>( new ThrowingPlayerChoiceContext(), heretic.Creature, 1M, heretic.Creature, null);
    Decimal num = await CreatureCmd.GainBlock(heretic.Creature, heretic.BlockGain, ValueProp.Move, null);
  }

  private async Task BelowMove(IReadOnlyList<Creature> targets)
  {
    Heretic monster = this;
    if (TestMode.IsOff)
    {
      NCreature creatureNode = monster.Creature.GetCreatureNode();
      Node2D specialNode = creatureNode?.GetSpecialNode<Node2D>("Visuals/SpineBoneNode");
      if (specialNode != null && creatureNode != null)
        specialNode.Position = targets.Count <= 0 ? Vector2.Left * 400f : Vector2.Right * (targets[0].GetCreatureNode().GlobalPosition.X - creatureNode.GlobalPosition.X) * 3f / creatureNode.Visuals.Scale;
      SfxCmd.Play("event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hidden_attack");
      await CreatureCmd.TriggerAnim(monster.Creature, "BurrowAttack", 0.25f);
      await Cmd.Wait(1f);
    }
    AttackCommand attackCommand = await DamageCmd.Attack((Decimal) monster.BelowDamage).FromMonster((MonsterModel) monster).WithHitFx("vfx/vfx_attack_slash").Execute((PlayerChoiceContext) null);
  }

  public async Task GetStunned()
  {
    Heretic heretic = this;
    heretic.IsStunned = true;
    await CreatureCmd.TriggerAnim(heretic.Creature, "Stun", 0.25f);
  }

  public async Task StillDizzyMove(IReadOnlyList<Creature> targets)
  {
    Heretic heretic = this;
    heretic.IsStunned = false;
    await CreatureCmd.TriggerAnim(heretic.Creature, "WakeUp", 0.25f);
  }

  public override CreatureAnimator GenerateAnimator(MegaSprite controller)
  {
    AnimState initialState = new AnimState("idle_loop", true);
    AnimState state1 = new AnimState("die");
    AnimState state2 = new AnimState("hurt");
    AnimState state3 = new AnimState("attack");
    AnimState state4 = new AnimState("stun");
    AnimState animState1 = new AnimState("stunned_loop", true);
    AnimState state5 = new AnimState("stunned_hurt");
    AnimState state6 = new AnimState("wake_up");
    AnimState state7 = new AnimState("burrow");
    AnimState animState2 = new AnimState("hidden_loop", true);
    AnimState state8 = new AnimState("hidden_attack");
    AnimState state9 = new AnimState("hidden_die");
    state7.NextState = animState2;
    state8.NextState = animState2;
    state3.NextState = initialState;
    state2.NextState = initialState;
    state4.NextState = animState1;
    state5.NextState = animState1;
    state6.NextState = initialState;
    CreatureAnimator animator = new CreatureAnimator(initialState, controller);
    animator.AddAnyState("Hit", state2, (Func<bool>) (() => !Creature.HasPower<BurrowedPower>() && !IsStunned));
    animator.AddAnyState("Hit", state5, (Func<bool>) (() => !Creature.HasPower<BurrowedPower>() && IsStunned));
    animator.AddAnyState("Dead", state1, (Func<bool>) (() => !Creature.HasPower<BurrowedPower>()));
    animator.AddAnyState("Dead", state9, (Func<bool>) (() => Creature.HasPower<BurrowedPower>()));
    animator.AddAnyState("Attack", state3, (Func<bool>) (() => !Creature.HasPower<BurrowedPower>()));
    animator.AddAnyState("BurrowAttack", state8, (Func<bool>) (() => Creature.HasPower<BurrowedPower>()));
    animator.AddAnyState("Burrow", state7);
    animator.AddAnyState("Stun", state4);
    animator.AddAnyState("WakeUp", state6);
    return animator;
  }

  protected override bool ShouldShowMoveInBestiary(string moveStateId)
  {
    return moveStateId != "DIZZY_MOVE";
  }
}