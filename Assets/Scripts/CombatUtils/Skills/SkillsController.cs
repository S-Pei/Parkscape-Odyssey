using System;
using System.Collections.Generic;

class SkillsController {
  public Dictionary<SkillName, Skill> skillsDB = new();

  public SkillsController() {
      Initialise();
  }

  private class SKNormalAttack : Skill {
    public override SkillName Name => SkillName.NORMAL_ATTACK;

    public override void Perform(Monster monster, List<Player> players) {
      players[0].TakeDamage(monster.BaseDamage);
    }
  }

  private class SKAoeNormalAttack : Skill {
    public override SkillName Name => SkillName.AOE_NORMAL_ATTACK;

    public override void Perform(Monster monster, List<Player> players) {
      players[0].TakeDamage(Convert.ToInt32(monster.BaseDamage * 0.8));
    }
  }

  private class SKBlock : Skill {
    public override SkillName Name => SkillName.BLOCK;

    public override void Perform(Monster monster, List<Player> players) {
      monster.IncreaseDef();
    }
  }

  private class SKEnrage : Skill {
    public override SkillName Name => SkillName.ENRAGE;

    public override void Perform(Monster monster, List<Player> players) {
      monster.TakeDamage(6);
      monster.Strengthen(10);
    }
  }

  // Catastrophe
  private class SKCatastrophe : Skill {
    public override SkillName Name => SkillName.CATASTROPHE;

    public override void Perform(Monster monster, List<Player> players) {
      foreach (Player player in players) {
        player.TakeDamage(25);
      }
      // can add burn effect once debuffs are implemented
    }
  }

  private void Initialise() {
    skillsDB.Add(SkillName.NORMAL_ATTACK, new SKNormalAttack());
    skillsDB.Add(SkillName.AOE_NORMAL_ATTACK, new SKAoeNormalAttack());
    skillsDB.Add(SkillName.BLOCK, new SKBlock());
    skillsDB.Add(SkillName.ENRAGE, new SKEnrage());
    skillsDB.Add(SkillName.CATASTROPHE, new SKCatastrophe());
  }

  public Skill Get(SkillName name) {
    if (skillsDB.ContainsKey(name)) {
      return skillsDB[name];
    }
    
    return null;
  }
}