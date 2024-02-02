using System.Collections.Generic;
using UnityEngine;

public class Monster {

  public readonly MonsterName name;

  public readonly Sprite img;

  public readonly int health;

  public readonly int baseDamage;

  public readonly List<Skill> skills;

  public readonly EnemyLevel level;


  public Monster(MonsterName name, Sprite img, int health, int baseDamage, List<Skill> skills, EnemyLevel level) {
    this.name = name;
    this.img = img;
    this.health = health;
    this.baseDamage = baseDamage;
    this.skills = skills;
    this.level = level;
  }
}