using System;
using System.Collections.Generic;
using UnityEngine;

class SkillsController {
  public Dictionary<SkillName, Skill> skillsDB = new();

  public SkillsController() {
      Initialise();
  }

  private class SKNormalAttack : Skill {
    public override SkillName Name => SkillName.NORMAL_ATTACK;

    public override void Perform(Monster monster, List<Player> players) {
      players[0].TakeDamage(monster.baseDamage);
    }
  }

  private class SKAoeNormalAttack : Skill {
    public override SkillName Name => SkillName.AOE_NORMAL_ATTACK;

    public override void Perform(Monster monster, List<Player> players) {
      players[0].TakeDamage(Convert.ToInt32(monster.baseDamage * 0.8));
    }
  }

  private class SKBlock : Skill {
    public override SkillName Name => SkillName.BLOCK;

    public override void Perform(Monster monster, List<Player> players) {
      monster.increaseDef();
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
    skillsDB.Add(SkillName.CATASTROPHE, new SKCatastrophe());
  }

  public Skill Get(SkillName name) {
    if (skillsDB.ContainsKey(name)) {
      return skillsDB[name];
    }

    return null;
  }
}