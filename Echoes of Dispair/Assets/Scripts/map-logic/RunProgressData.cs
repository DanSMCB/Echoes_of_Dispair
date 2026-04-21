using System;
using System.Collections.Generic;

[Serializable]
public class RunProgressData
{
    public MapProgressState currentMapState = MapProgressState.Start;

    public bool firstBattleWon = false;
    public bool rewardChosen = false;
    public bool finalBattleUnlocked = false;

    public List<string> currentRewardCardOptions = new List<string>();
}