using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct KillLogInformation
{
    public PlayerInstance killer;
    public PlayerInstance victim;
    public KillLogInformation(PlayerInstance killer, PlayerInstance victim)
    {
        this.killer = killer;
        this.victim = victim;
    }
}
public class GameKillLogUI : MonoBehaviour
{
    public Queue<KillLogInformation> killLogs = new Queue<KillLogInformation>();
    private bool IsShowing = false;

    [SerializeField]
    private GameKillLogUIItem GameKillLogUIItemPrefab;

    [SerializeField]
    private Transform spawnParent;

    public void AddKillLog(KillLogInformation log)
    {
        killLogs.Enqueue(log);
    }

    private void Update()
    {
        if(killLogs.Count > 0)
        {
            var log = killLogs.Dequeue();
            var logItem = Instantiate(GameKillLogUIItemPrefab, spawnParent);
            if(log.killer.TeamNumber == 1)
            {
                logItem.killerText.color = Color.red;
            }
            else
            {
                logItem.killerText.color = Color.blue;
            }

            if(log.victim.TeamNumber == 1)
            {
                logItem.victimText.color = Color.red;
            }
            else
            {
                logItem.victimText.color = Color.blue;
            }
        }
    }
}
