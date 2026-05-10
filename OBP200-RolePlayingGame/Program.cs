using System.Text;

namespace OBP200_RolePlayingGame;


class Program
{
    // ======= Globalt tillstånd  =======

    // Spelarens "databas": alla värden som strängar
    // index: 0 Name, 1 Class, 2 HP, 3 MaxHP, 4 ATK, 5 DEF, 6 GOLD, 7 XP, 8 LEVEL, 9 POTIONS, 10 INVENTORY (semicolon-sep)
    static Player? CurrentPlayer; //varaibel 

    // Rum: [type, label]
    // types: battle, treasure, shop, rest, boss
    static List<string[]> Rooms = new List<string[]>();

    // Fiendemallar: [type, name, HP, ATK, DEF, XPReward, GoldReward]
    static List<string[]> EnemyTemplates = new List<string[]>();

    // Status för kartan
    static int CurrentRoomIndex = 0;

    // Random
    static Random Rng = new Random();

    // ======= Main =======

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitEnemyTemplates();

        while (true)
        {
            ShowMainMenu();
            Console.Write("Välj: ");
            var choice = (Console.ReadLine() ?? "").Trim();

            if (choice == "1")
            {
                StartNewGame();
                RunGameLoop();
            }
            else if (choice == "2")
            {
                Console.WriteLine("Avslutar...");
                return;
            }
            else
            {
                Console.WriteLine("Ogiltigt val.");
            }

            Console.WriteLine();
        }
    }

    // ======= Meny & Init =======

    static void ShowMainMenu()
    {
        Console.WriteLine("=== Text-RPG ===");
        Console.WriteLine("1. Nytt spel");
        Console.WriteLine("2. Avsluta");
    }

    static void StartNewGame()
    {
        Console.Write("Ange namn: ");
        var name = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Namnlös";

        Console.WriteLine("Välj klass: 1) Warrior  2) Mage  3) Rogue");
        Console.Write("Val: ");
        var k = (Console.ReadLine() ?? "").Trim();

        string cls = "Warrior";
        int hp = 0, maxhp = 0, atk = 0, def = 0;
        int potions = 0, gold = 0;
        
        switch (k)
        {
            case "1": // Warrior: tankig
                cls = "Warrior";
                maxhp = 40; hp = 40; atk = 7; def = 5; potions = 2; gold = 15;
                break;
            case "2": // Mage: hög damage, låg def
                cls = "Mage";
                maxhp = 28; hp = 28; atk = 10; def = 2; potions = 2; gold = 15;
                break;
            case "3": // Rogue: krit-chans
                cls = "Rogue";
                maxhp = 32; hp = 32; atk = 8; def = 3; potions = 3; gold = 20;
                break;
            default:
                cls = "Warrior";
                maxhp = 40; hp = 40; atk = 7; def = 5; potions = 2; gold = 15;
                break;
        }
        CurrentPlayer = new Player(name, hp, atk, def, cls) // Skapar Player som objekt
        {
            Gold = gold, 
            Level = 1, 
            Potions = potions,
            Experience = 0,
            MaxHealth = maxhp
        };
        
        // Initiera karta (linjärt äventyr)
        Rooms.Clear();
        Rooms.Add(new[] { "battle", "Skogsstig" });
        Rooms.Add(new[] { "treasure", "Gammal kista" });
        Rooms.Add(new[] { "shop", "Vandrande köpman" });
        Rooms.Add(new[] { "battle", "Grottans mynning" });
        Rooms.Add(new[] { "rest", "Lägereld" });
        Rooms.Add(new[] { "battle", "Grottans djup" });
        Rooms.Add(new[] { "boss", "Urdraken" });

        CurrentRoomIndex = 0;

        Console.WriteLine($"Välkommen, {name} the {cls}!");
        ShowStatus();
    }

    static void RunGameLoop()
    {
        while (true)
        {
            var room = Rooms[CurrentRoomIndex];
            Console.WriteLine($"--- Rum {CurrentRoomIndex + 1}/{Rooms.Count}: {room[1]} ({room[0]}) ---");

            bool continueAdventure = EnterRoom(room[0]);
            
            if (IsPlayerDead())
            {
                Console.WriteLine("Du har stupat... Spelet över.");
                break;
            }
            
            if (!continueAdventure)
            {
                Console.WriteLine("Du lämnar äventyret för nu.");
                break;
            }

            CurrentRoomIndex++;
            
            if (CurrentRoomIndex >= Rooms.Count)
            {
                Console.WriteLine();
                Console.WriteLine("Du har klarat äventyret!");
                break;
            }
            
            Console.WriteLine();
            Console.WriteLine("[C] Fortsätt     [Q] Avsluta till huvudmeny");
            Console.Write("Val: ");
            var post = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (post == "Q")
            {
                Console.WriteLine("Tillbaka till huvudmenyn.");
                break;
            }

            Console.WriteLine();
        }
    }

    // ======= Rumshantering =======

    static bool EnterRoom(string type)
    {
        switch ((type ?? "battle").Trim())
        {
            case "battle":
                return DoBattle(isBoss: false);
            case "boss":
                return DoBattle(isBoss: true);
            case "treasure":
                return DoTreasure();
            case "shop":
                return DoShop();
            case "rest":
                return DoRest();
            default:
                Console.WriteLine("Du vandrar vidare...");
                return true;
        }
    }

    // ======= Strid =======

    static bool DoBattle(bool isBoss)
    {
        Enemy enemy = GenerateEnemy(isBoss);
        Console.WriteLine($"En {enemy.Name} dyker upp! (HP {enemy.Health}, ATK {enemy.Attack}, DEF {enemy.Defense})");
        

        while (enemy.Health > 0 && !IsPlayerDead())
        {
            Console.WriteLine();
            ShowStatus();
            Console.WriteLine($"Fiende: {enemy.Name} HP={enemy.Health}");
            Console.WriteLine("[A] Attack   [X] Special   [P] Dryck   [R] Fly");
            if (isBoss) Console.WriteLine("(Du kan inte fly från en boss!)");
            Console.Write("Val: ");

            var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (cmd == "A")
            {
                int damage = CalculatePlayerDamage(enemy.Defense);
                enemy.TakeDamage(damage);
                Console.WriteLine($"Du slog {enemy.Name} för {damage} skada.");
            }
            else if (cmd == "X")
            {
                int special = UseClassSpecial(enemy.Defense, isBoss);
                enemy.TakeDamage(special);
                Console.WriteLine($"Special! {enemy.Name} tar {special} skada.");
            }
            else if (cmd == "P")
            {
                UsePotion();
            }
            else if (cmd == "R" && !isBoss)
            {
                if (TryRunAway())
                {
                    Console.WriteLine("Du flydde!");
                    return true; // fortsätt äventyr
                }
                else
                {
                    Console.WriteLine("Misslyckad flykt!");
                }
            }
            else
            {
                Console.WriteLine("Du tvekar...");
            }

            if (enemy.Health <= 0) break;

            // Fiendens tur
            int enemyDamage = CalculateEnemyDamage(enemy);
            ApplyDamageToPlayer(enemyDamage);
            Console.WriteLine($"{enemy.Name} anfaller och gör {enemyDamage} skada!");
        }

        if (IsPlayerDead())
        {
            return false; // avsluta äventyr
        }
        
        AddPlayerXp(enemy.ExperienceReward);
        AddPlayerGold(enemy.GoldReward);
            
        Console.WriteLine($"Seger! +{enemy.ExperienceReward} XP, +{enemy.GoldReward} guld.");
        MaybeDropLoot(enemy.Name);
      
        return true;
    }

    static Enemy GenerateEnemy (bool isBoss)
    {
        if (isBoss)
        {
            // Boss-mall
            return new Enemy( "boss", "Urdraken", 55, 9, 4, 30, 50);
        }
       
        var template = EnemyTemplates[Rng.Next(EnemyTemplates.Count)];
            
        // Slmumpmässig justering av stats
        int hp = ParseInt(template[2], 10) + Rng.Next(-1, 3);
        int atk = ParseInt(template[3], 3) + Rng.Next(0, 2);
        int def = ParseInt(template[4], 0) + Rng.Next(0, 2);
        int xp = ParseInt(template[5], 4) + Rng.Next(0, 3);
        int gold = ParseInt(template[6], 2) + Rng.Next(0, 3);
        return new Enemy( template[0], template[1], hp, atk, def, xp, gold);
    }

    static void InitEnemyTemplates()
    {
        EnemyTemplates.Clear();
        EnemyTemplates.Add(new[] { "beast", "Vildsvin", "18", "4", "1", "6", "4" });
        EnemyTemplates.Add(new[] { "undead", "Skelett", "20", "5", "2", "7", "5" });
        EnemyTemplates.Add(new[] { "bandit", "Bandit", "16", "6", "1", "8", "6" });
        EnemyTemplates.Add(new[] { "slime", "Geléslem", "14", "3", "0", "5", "3" });
    }

    static int CalculatePlayerDamage(int enemyDef)
    {
        if (CurrentPlayer == null) return 0;

        int atk = CurrentPlayer.Attack;
        string cls = CurrentPlayer.ClassType;

        // Beräkna grundskada
        int baseDmg = Math.Max(1, atk - (enemyDef / 2));
        int roll = Rng.Next(0, 3); // liten variation

        switch (cls.Trim())
        {
            case "Warrior":
                baseDmg += 1; // warrior buff
                break;
            case "Mage":
                baseDmg += 2; // mage buff
                break;
            case "Rogue":
                baseDmg += (Rng.NextDouble() < 0.2) ? 4 : 0; // rogue crit-chans
                break;
        }

        return Math.Max(1, baseDmg + roll);
    }

    static int UseClassSpecial(int enemyDef, bool vsBoss)
    {
        if (CurrentPlayer == null) return 0;
        
        string cls = CurrentPlayer.ClassType;
        int specialDmg = 0;

        // Hantering av specialförmågor
        if (cls == "Warrior")
        {
            // Heavy Strike: hög skada men självskada
            Console.WriteLine("Warrior använder Heavy Strike!");
            int atk = CurrentPlayer.Attack;
            specialDmg = Math.Max(2, atk + 3 - enemyDef);
            ApplyDamageToPlayer(2); // självskada
        }
        else if (cls == "Mage")
        {
            // Fireball: stor skada, kostar guld
            int gold = CurrentPlayer.Gold;
            if (gold >= 3)
            {
                Console.WriteLine("Mage kastar Fireball!");
                CurrentPlayer.Gold -= 3;
                int atk = CurrentPlayer.Attack;
                specialDmg = Math.Max(3, atk + 5 - (enemyDef / 2));
            }
            else
            {
                Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
                specialDmg = 0;
            }
        }
        else if (cls == "Rogue")
        {
            // Backstab: chans att ignorera försvar, hög risk/hög belöning
            if (Rng.NextDouble() < 0.5)
            {
                Console.WriteLine("Rogue utför en lyckad Backstab!");
                int atk = CurrentPlayer.Attack;
                specialDmg = Math.Max(4, atk + 6);
            }
            else
            {
                Console.WriteLine("Backstab misslyckades!");
                specialDmg = 1;
            }
        }
        // Dämpa skada mot bossen
        if (vsBoss)
        {
            specialDmg = (int)Math.Round(specialDmg * 0.8);
        }

        return Math.Max(0, specialDmg);
    }

    static int CalculateEnemyDamage(Character enemy)
    {
        if (CurrentPlayer == null) return 0;

        int def = CurrentPlayer.Defense;
        int roll = Rng.Next(0, 3);

        int baseDmg = enemy.DealDamage(); 
        int dmg = Math.Max(1, baseDmg - (def / 2)) + roll;

        if (Rng.NextDouble() < 0.1)
            dmg = Math.Max(1, dmg - 2);

        return dmg;
    }

    static void ApplyDamageToPlayer(int dmg)
    {
        if (CurrentPlayer == null) return; 
            //Uppdaterar spelarens hälsa via Player-objektet
            CurrentPlayer.TakeDamage(dmg);
    }

    static void UsePotion()
    {
        if (CurrentPlayer == null) return;
        int pot = CurrentPlayer.Potions;
        if (pot <= 0)
        {
            Console.WriteLine("Du har inga drycker kvar.");
            return;
        }

        int hp = CurrentPlayer.Health;
        int maxhp = CurrentPlayer.MaxHealth;

        // Helning av spelaren
        int heal = 12;
        int newHp = Math.Min(maxhp, hp + heal);
        CurrentPlayer.Health = newHp;
       CurrentPlayer.Potions--;

        Console.WriteLine($"Du dricker en dryck och återfår {newHp - hp} HP.");
    }

    static bool TryRunAway()
    {
        if (CurrentPlayer == null) return false;
        
        // Flyktschans baserad på karaktärsklass
        string cls = CurrentPlayer.ClassType;
        double chance = 0.25;
        if (cls == "Rogue") chance = 0.5;
        if (cls == "Mage") chance = 0.35;
        return Rng.NextDouble() < chance;
    }

    static bool IsPlayerDead()
    {
        if (CurrentPlayer == null) return true;
            // Kollar om spelaren är död via Player-objektet
            return CurrentPlayer.Health <= 0;
    }

    static void AddPlayerXp(int amount)
    {
        if (CurrentPlayer == null) return; 
            CurrentPlayer.Experience += amount;
            MaybeLevelUp();
    }

    static void AddPlayerGold(int amount)
    {
        if (CurrentPlayer == null) return; 
            // Uppdaterar spelarens Guld via Player-objektet
            CurrentPlayer.Gold += Math.Max(0, amount);
    }

    static void MaybeLevelUp()
    {
        if (CurrentPlayer == null) return;
        // Hanterar level up via Player-objektet
            int lvl = CurrentPlayer.Level;
            int nextThreshold = lvl == 1 ? 10 : (lvl == 2 ? 25 : (lvl == 3 ? 45 : lvl * 20));

            if (CurrentPlayer.Experience >= nextThreshold)
            {
                CurrentPlayer.Level++;

                switch (CurrentPlayer.ClassType)
                {
                    case "Warrior":
                        CurrentPlayer.MaxHealth += 6;
                        CurrentPlayer.Attack += 2;
                        CurrentPlayer.Defense += 2;
                        break;
                    case "Mage":
                        CurrentPlayer.MaxHealth += 4;
                        CurrentPlayer.Attack += 4;
                        CurrentPlayer.Defense += 1;
                        break;
                    case "Rogue":
                        CurrentPlayer.MaxHealth += 5;
                        CurrentPlayer.Attack += 3;
                        CurrentPlayer.Defense += 1;
                        break;
                    default:
                        CurrentPlayer.MaxHealth += 4;
                        CurrentPlayer.Attack += 3;
                        CurrentPlayer.Defense += 1;
                        break;
                }

                CurrentPlayer.Health = CurrentPlayer.MaxHealth;
                Console.WriteLine($"Du når nivå {CurrentPlayer.Level}! Värden ökade och HP återställd.");
            }
    }

    static void MaybeDropLoot(string enemyName)
    {
        // Enkel loot-regel
        if (Rng.NextDouble() < 0.35)
        {
            string item = enemyName.Contains("Urdraken") ? "Dragon Scale" : "Minor Gem";
            
            //lägger i list istället för array
            CurrentPlayer?.Inventory.Add(item);

            Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
        }
    }

    // ======= Rumshändelser =======

    static bool DoTreasure()
    {
        if(CurrentPlayer == null) return false;
        
        Console.WriteLine("Du hittar en gammal kista...");
        if (Rng.NextDouble() < 0.5)
        {
            int gold = Rng.Next(8, 15);
            AddPlayerGold(gold);
            Console.WriteLine($"Kistan innehåller {gold} guld!");
        }
        else
        {
            var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            string found = items[Rng.Next(items.Length)];
            
            CurrentPlayer?.Inventory.Add(found);    
            Console.WriteLine($"Du plockar upp: {found}");
        }
        return true;
    }

    static bool DoShop()
    {
        if (CurrentPlayer == null) return  false;
           Console.WriteLine("En vandrande köpman erbjuder sina varor:");
        while (true)
        {
            //värden från CurrentPlayer
            Console.WriteLine($"Guld: {CurrentPlayer.Gold} | Drycker: {CurrentPlayer.Potions}");
            Console.WriteLine("1) Köp dryck (10 guld)");
            Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
            Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
            Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
            Console.WriteLine("5) Lämna butiken");
            Console.Write("Val: ");
            var val = (Console.ReadLine() ?? "").Trim();

            if (val == "1")
            {
                if (CurrentPlayer.Gold >= 10)
                { 
                    CurrentPlayer.Gold -= 10;
                    CurrentPlayer.Potions++;
                    Console.WriteLine("Du köper en dryck. ");
                }
                else Console.WriteLine("Du har inte råd. ");
            }
            else if (val == "2")
            {
                if (CurrentPlayer.Gold >= 25)
                {
                    CurrentPlayer.Gold -= 25;
                    CurrentPlayer.Attack += 2;
                    Console.WriteLine("Du köper ett bättre vapen (+2 ATK). ");
                }
                else Console.WriteLine("Du har inte råd");
            }
            else if (val == "3")
            {
                if (CurrentPlayer.Gold >= 25)
                {
                    CurrentPlayer.Gold -= 25;
                    CurrentPlayer.Defense += 2;
                    Console.WriteLine("Du köper rustning (+2 DEF). ");
                }
                else Console.WriteLine("Du har inte råd. ");
            }
            else if (val == "4")
            {
                SellMinorGems();
            }
            else if (val == "5")
            {
                Console.WriteLine("Du säger adjö till köpmannen.");
                break;
            }
            else
            {
                Console.WriteLine("Köpmannen förstår inte ditt val.");
            }
        }
        return true;
    }
    
    static void SellMinorGems()
    {
        if (CurrentPlayer == null || CurrentPlayer.Inventory.Count == 0)
        {
            Console.WriteLine("Du har inga föremål att sälja.");
            return;
        }

        int count = CurrentPlayer.Inventory.Count(x => x == "Minor Gem");
        if (count == 0)
        {
            Console.WriteLine("Inga 'Minor Gem' i väskan.");
            return;
        }

        CurrentPlayer.Inventory.RemoveAll(x => x == "Minor Gem");

        AddPlayerGold(count * 5);
        Console.WriteLine($"Du säljer {count} st Minor Gem för {count * 5} guld.");
    }

    static bool DoRest()
    {
        if (CurrentPlayer == null) return false;
        
        Console.WriteLine("Du slår läger och vilar.");
        CurrentPlayer.Health = CurrentPlayer.MaxHealth;
        Console.WriteLine("HP återställt till max.");
        return true;
    }

    // ======= Status =======

    static void ShowStatus()
    {
        if (CurrentPlayer == null) return;
        // Uppdaterar spelarens hälsa via Player-objektet
        Console.WriteLine($"[{CurrentPlayer.Name} | {CurrentPlayer.ClassType}]  HP {CurrentPlayer.Health}/{CurrentPlayer.MaxHealth}  ATK {CurrentPlayer.Attack}  DEF {CurrentPlayer.Defense}  LVL {CurrentPlayer.Level}  XP {CurrentPlayer.Experience}  Guld {CurrentPlayer.Gold}  Drycker {CurrentPlayer.Potions}");
        if (CurrentPlayer.Inventory.Count > 0)
        {
            Console.WriteLine($"Väska: {string.Join(" , ", CurrentPlayer.Inventory)}");
        }
    }
    
    // ======= Hjälpmetoder =======

    static int ParseInt(string s, int fallback)
    {
        try
        {
            int value = Convert.ToInt32(s);
            return value;
        }
        catch (Exception)
        {
            return fallback;
        }
    }
}
