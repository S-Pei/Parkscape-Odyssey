public class Player
{
    // Properties
    public string Name { get; }
    public string Id { get; }
    public string Role { get; }
    public int Speed { get; }
    public int MaxHealth { get; }
    public int CurrentHealth { get; }
    public int Mana { get; }
    public int MaxMana { get; }
    public int Strength { get; }
    public string Description { get; }

    // Multipliers
    public float AttackMultiplier { get; set; }
    public float DefenceMultiplier { get; set; }

    // Constants
    private const int BASESTRENGTH = 0;
    private const int BASEMULTIPLIER = 1;

    // Constructor
    public Player(string name, string id, string role, int speed, int maxHealth, int maxMana, string description = "") {
        Name = name;
        Id = id;
        Role = role;
        Speed = speed;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        MaxMana = maxMana;
        Mana = maxMana;
        Strength = BASESTRENGTH;
        AttackMultiplier = BASEMULTIPLIER;
        DefenceMultiplier = BASEMULTIPLIER;
        Description = description;
    }

}
