public class EconomyLogic
{
    public int Coins { get; private set; }

    public EconomyLogic(int startCoins) => Coins = startCoins;

    public bool TrySpend(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        return true;
    }

    public void AddCoins(int amount) => Coins += amount;
    public void SetCoins(int amount) => Coins  = amount;
}
