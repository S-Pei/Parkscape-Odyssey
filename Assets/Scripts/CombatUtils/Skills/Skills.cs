using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

class Skills {
  public Dictionary<SkillName, Skill> skillsDB = new();

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

  private Skill Get(SkillName name) {
    if (skillsDB.Count == 0) {
      Initialise();
    }

    if (skillsDB.ContainsKey(name)) {
      return skillsDB[name];
    }

    return null;
  }
}