using System.Collections.Generic;

abstract class Skill {
  abstract public SkillName Name { get; }

  public abstract void Perform(Monster monster, List<Player> players);
}