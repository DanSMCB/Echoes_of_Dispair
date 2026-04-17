using UnityEngine;

public class CityState : MonoBehaviour
{
    public int cityIndex;
    public int elevationLevel;

    public DisasterInstance pendingDisaster;
    public DisasterInstance activeDisaster;

    public void Initialize(int index, int cityElevation)
    {
        cityIndex = index;
        elevationLevel = cityElevation;

        pendingDisaster = null;
        activeDisaster = null;
    }
}
