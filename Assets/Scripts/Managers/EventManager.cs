using UnityEngine;
using System.Collections.Generic;
using Gabadie.GFSM;

public class EventManager : MonoBehaviour
{
    public static EventManager Inst;
    public Queue<Event> eventQueue = new();

    private void Awake()
    {
        //Singleton boilerplate
        if (Inst == null)
            Inst = this;
        else if (Inst != this)
            Destroy(this);
    }

    private void Update()
    {
        while (eventQueue.Count > 0)
        {
            eventQueue.Dequeue().Execute();
        }
    }

    public void Send(Event e)
    {
        eventQueue.Enqueue(e);
    }
}

public abstract class Event
{
    public abstract void Execute();
}

public class PlayerTransitionEvent : Event
{
    Player player;
    InputFrame input;
    State target;

    public PlayerTransitionEvent(Player _player, State _target)
    {
        player = _player;
        target = _target;
    }

    public override void Execute()
    {
        player.animator.PlayAnim(target.Name);
    }
}