namespace CeeLoPlugin.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int Roll { get; set; } // Added Roll property
        public int Score { get; set; } // Retains Score for gameplay
        public int FinalScore { get; set; } // Added FinalScore property
        public Player(string name)
        {
            Name = name;
            Roll = 0;
            Score = 0;
        }
    }
}
