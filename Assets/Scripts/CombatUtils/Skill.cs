using System.Collections.Generic;

abstract class Skill {
  public readonly SkillName name;

  public abstract void Perform(Monster monster, List<Player> players);
}