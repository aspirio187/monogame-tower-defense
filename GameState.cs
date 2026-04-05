namespace monogame_funny_game;

public class GameState
{
    public int Lives;
    public int Currency;
    public bool GameOver;
    public bool GameWon;

    public GameState()
    {
        Lives = 20;
        Currency = 150;
        GameOver = false;
        GameWon = false;
    }

    public bool TryPurchase(int cost)
    {
        if (Currency >= cost)
        {
            Currency -= cost;
            return true;
        }
        return false;
    }

    public void EnemyKilled(int reward)
    {
        Currency += reward;
    }

    public void EnemyReachedEnd()
    {
        Lives--;
        if (Lives <= 0)
            GameOver = true;
    }

    public void Reset()
    {
        Lives = 20;
        Currency = 150;
        GameOver = false;
        GameWon = false;
    }
}
