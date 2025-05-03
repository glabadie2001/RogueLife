using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public static NPCManager inst;

    void Awake()
    {
        if (inst == null)
            inst = this;
        else if (inst != this)
            Destroy(this);
    }

    public NPCInfo GenerateRandomNPC()
    {
        return new NPCInfo("John Doe");
    }

    void Spawn()
    {

    }
}