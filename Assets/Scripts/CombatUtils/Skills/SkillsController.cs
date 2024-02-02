using System.Collections.Generic;
using UnityEngine;

class SkillsController {
  public Dictionary<SkillName, Skill> skillsDB = new();

  public SkillsController() {
      Initialise();
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
    skillsDB.Add(SkillName.CATASTROPHE, new SKCatastrophe());
  }

  public Skill Get(SkillName name) {
    if (skillsDB.ContainsKey(name)) {
      return skillsDB[name];
    }

    return null;
  }
}