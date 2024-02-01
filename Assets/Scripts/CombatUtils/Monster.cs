using System.Collections.Generic;
using UnityEngine;

class Monster {

  public readonly MonsterName name;

  public readonly Sprite img;

  public readonly int health;

  public readonly int baseDamage;

  public readonly List<Skill> skills;


  public Monster(MonsterName name, Sprite img, int health, int baseDamage, List<SkillName> skills) {
    this.name = name;
    this.img = img;
    this.health = health;
    this.baseDamage = baseDamage;
    // this.skills = skills;
  }
}