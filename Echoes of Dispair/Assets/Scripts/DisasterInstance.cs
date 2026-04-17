using UnityEngine;

public class DisasterInstance
{
    public DisasterType type = DisasterType.None;
    public int damagePerTurn = 0;
    public int turnsUntilActivation = 0;
    public int activeTurnsRemaining = 0;

    public bool IsValid()
    {
        return type != DisasterType.None;
    }

    public bool IsPending()
    {
        return IsValid() && turnsUntilActivation > 0;
    }

    public bool IsActive()
    {
        return IsValid() && turnsUntilActivation <= 0 && activeTurnsRemaining > 0;
    }
}
