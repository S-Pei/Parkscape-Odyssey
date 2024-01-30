public class PlayerFactory {
    // Constants
    private const int BASEMANA = 3;
    private const int BASEHEALTH = 75;
    private const int BASESPEED = 5;

    // Factory Methods
    public static Player CreateMage(string name) {
        return new Player(name: name, 
                          role: "Mage", 
                          speed: BASESPEED, 
                          maxHealth: BASEHEALTH - 10, 
                          maxMana: BASEMANA + 1);
    }

    public static Player CreateWarrior(string name) {
        return new Player(name: name, 
                          role: "Warrior", 
                          speed: BASESPEED - 2, 
                          maxHealth: BASEHEALTH + 25, 
                          maxMana: BASEMANA);
    }

    public static Player CreateRogue(string name) {
        return new Player(name: name, 
                          role: "Rogue", 
                          speed: BASESPEED + 2, 
                          maxHealth: BASEHEALTH - 15, 
                          maxMana: BASEMANA);
    }

    public static Player CreateCleric(string name) {
        return new Player(name: name, 
                          role: "Cleric", 
                          speed: BASESPEED, 
                          maxHealth: BASEHEALTH - 10, 
                          maxMana: BASEMANA);
    }

    public static Player CreateFaerie(string name) {
        return new Player(name: name, 
                          role: "Faerie", 
                          speed: BASESPEED + 3, 
                          maxHealth: BASEHEALTH - 30, 
                          maxMana: BASEMANA + 2);
    }

    public static Player CreateScout(string name) {
        return new Player(name: name, 
                          role: "Scout", 
                          speed: BASESPEED - 1, 
                          maxHealth: BASEHEALTH - 10, 
                          maxMana: BASEMANA);
    }
}