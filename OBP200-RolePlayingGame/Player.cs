using System.Runtime.InteropServices;

namespace OBP200_RolePlayingGame;
public class Player : Character //här ärver Player från Character 

{
    public string ClassType { get; set; }
    public int Gold { get; set; }
    public int Level { get; set; }
    public int Potions { get; set; } 
    public int Experience { get; set; }
    public int MaxHealth { get; set; } 
    
    public Player (string name, int health, int attack, int defence, string classType) 
     :base(name, health, attack, defence)
    {
        ClassType = classType;
        
    }
}
