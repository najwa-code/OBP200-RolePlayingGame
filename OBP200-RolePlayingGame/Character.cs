namespace OBP200_RolePlayingGame;
// Både spelare och fiender delar gemensamma egenskaper,
// därför används en basklass Character
public class Character
{
    public string Name { get; set; } 
    public int Health { get; set; }
    public int Attack  { get; set; }
    public int Defense { get; set; }

    public Character (string name, int health, int attack, int defense)
    {
        Name = name;
        Health = health;
        Attack = attack;
        Defense = defense;
    }
}