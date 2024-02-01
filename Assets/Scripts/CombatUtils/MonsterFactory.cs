using System;
using System.Collections.Generic;

public sealed class MonsterFactory {

  private MonsterFactory() {}

  private static readonly MonsterFactory singleton = new();

  public static MonsterFactory Instance() {
      return singleton;
  }

  private readonly static SkillsController skillsController = new();

  private static int IntRandomizer(int min, int max) {
    Random random = new();
    double multiplier = random.NextDouble();
    return min + Convert.ToInt32((max - min) * multiplier);
  }


  public static Monster CreateGoblin() {
    List<Skill> skills = new()
    {
        skillsController.Get(SkillName.NORMAL_ATTACK),
        skillsController.Get(SkillName.TAUNT),
        skillsController.Get(SkillName.BLOCK),
        skillsController.Get(SkillName.ENRAGE)
    };

    int health = IntRandomizer(30, 55);

    return new Monster(name: MonsterName.GOBLIN, 
                       health: health, 
                       baseDamage: IntRandomizer(5, 10), 
                       skills: skills,
                       level: health >= 50 ? EnemyLevel.MEDIUM : EnemyLevel.EASY);
  }

  public static Monster CreateDragon() {
    List<Skill> skills = new()
    {
        skillsController.Get(SkillName.NORMAL_ATTACK),
        skillsController.Get(SkillName.AOE_NORMAL_ATTACK),
        skillsController.Get(SkillName.BLOCK),
        skillsController.Get(SkillName.ENRAGE),
        skillsController.Get(SkillName.FLY),
        skillsController.Get(SkillName.CATASTROPHE),
    };

    int health = IntRandomizer(250, 400);

    return new Monster(name: MonsterName.DRAGON, 
                       health: IntRandomizer(250, 400), 
                       baseDamage: IntRandomizer(15, 30), 
                       skills: skills,
                       level: health >= 300 ? EnemyLevel.HARD : EnemyLevel.BOSS);
  }
}