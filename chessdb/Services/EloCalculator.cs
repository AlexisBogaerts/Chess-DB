using System;
public interface IRankingCalculator
{
    (int newWhiteElo, int newBlackElo) Calculate(int whiteElo, int blackElo, double whiteScore, int kFactor = 20);
}

public class EloCalculator : IRankingCalculator
{
    public (int newWhiteElo, int newBlackElo) Calculate(int whiteElo, int blackElo, double whiteScore, int kFactor = 20)
    {
        double expectedWhite = 1.0 / (1.0 + Math.Pow(10.0, (blackElo - whiteElo) / 400.0));
        double expectedBlack = 1.0 - expectedWhite;

        double actualWhite = whiteScore;
        double actualBlack = 1.0 - actualWhite;

        int newWhite = (int)Math.Round(whiteElo + kFactor * (actualWhite - expectedWhite));
        int newBlack = (int)Math.Round(blackElo + kFactor * (actualBlack - expectedBlack));
        return (newWhite, newBlack);
    }
}