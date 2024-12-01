namespace CeeLoPlugin.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int InitialRoll { get; set; }

        public Player(string name)
        {
            Name = name;
            InitialRoll = 0;
        }
    }
}
