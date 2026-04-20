using System;

[Serializable]
public class RunProgressData
{
    public MapProgressState currentMapState = MapProgressState.Start;

    public bool firstBattleWon = false;
    public bool rewardChosen = false;
    public bool finalBattleUnlocked = false;
}