using MoreMountains.Tools;
using UnityEngine;

public class GameSequenceManager : MMSingleton<GameSequenceManager>
{
    private void Start()
    {
        MusicManager.Instance.PlayMusic("GameMusic");       
    }

    public void BeginBattle()
    {
        MusicManager.Instance.PlayMusic("Bettle");
    }
}
