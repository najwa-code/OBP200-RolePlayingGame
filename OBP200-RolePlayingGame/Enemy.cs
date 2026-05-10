namespace OBP200_RolePlayingGame;

public class Enemy : Character //enemy ärver från Character 
{
    public string Type { get; set; }
    public int ExperienceReward { get; set; }
    public int GoldReward { get; set; }

    public Enemy(string type, string name, int health, int attack, int defense, int experienceReward, int goldReward)
        : base(name, health, attack, defense)
    {
        Type = type;
        ExperienceReward = experienceReward;
        GoldReward = goldReward;
    }

    public override int DealDamage()
    {
        return Attack;
    }
}

