namespace HappyTokenApi.Models
{
    public class DailyRewards
    {
        public Wallet Wallet { get; set; }

        public Happiness Happiness { get; set; }

        public DailyRewards()
        {
            Wallet = new Wallet();
            Happiness = new Happiness();
        }
    }
}
