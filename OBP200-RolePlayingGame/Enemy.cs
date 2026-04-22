using System.Xml;

namespace OBP200_RolePlayingGame;
//enemy ärver från Character 

public class Enemy : Character
{

    public Enemy(string name, int health, int attack, int defense)
        : base(name, health, attack, defense)
    {
    }
}

