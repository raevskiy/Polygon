/// <summary>
/// Saved state of the airship "Diana" (see KopliSoft.Vehicles.AirshipState).
/// </summary>
[System.Serializable]
public class AirshipSaveData
{
    public bool moored;
    public bool captainInside = true;
    public bool landingComplete;
    public bool talkingToCaptainUnlocked;
}
