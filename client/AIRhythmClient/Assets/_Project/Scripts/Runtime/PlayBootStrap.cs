using UnityEngine;

public class PlayBootStrap : MonoBehaviour
{
    [SerializeField]
    private SongConductor conductor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ManualStart()
    {
        if (conductor != null)
            conductor.StartSong();
    }
}
