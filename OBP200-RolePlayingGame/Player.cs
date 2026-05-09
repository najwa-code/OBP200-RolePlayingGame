
namespace OBP200_RolePlayingGame;
public class Player : Character //här ärver Player från Character 

{
    public string ClassType { get; set; }
    public int Gold { get; set; }
    public int Level { get; set; }
    public int Potions { get; set; } 
    public int Experience { get; set; } 
    public int MaxHealth { get; set; } 
    
    //lista för att hålla reda på föremål
    public List<string> Inventory { get; set; } = new List<string>();
    
    public Player (string name, int health, int attack, int defence, string classType) 
     :base(name, health, attack, defence)
    {
        ClassType = classType;
        // start föremål
        Inventory.Add("Träsvärd");
        Inventory.Add("TyggutRustning");
    }
}
