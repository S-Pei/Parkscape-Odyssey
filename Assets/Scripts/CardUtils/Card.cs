using UnityEngine;

public class Card {
  public readonly string name;
  public readonly Sprite img;
  public readonly string stats;
  public readonly int cost;
  public readonly string type;

  public Card(string name, Sprite img, string stats, int cost, string type) {
    this.name = name;
    this.img = img;
    this.stats = stats;
    this.cost = cost;
    this.type = type;
  }
}