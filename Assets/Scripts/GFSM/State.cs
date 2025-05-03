using System;
using UnityEngine;

namespace Gabadie.GFSM
{
    public struct State : IState
    {
        [SerializeField] string _name;
        public string Name => _name;

        Action _onEnter;
        Action<float> _update;
        Action<State> _onExit;

        [SerializeField] bool _complete;
        public bool Complete => _complete;

        [SerializeField] bool _isAny;
        public bool IsAny => _isAny;

        public static IState Any => new State("Any", null, null, null) { _isAny = true };

        public State(string name, Action onEnter = null, Action<float> update = null, Action<State> onExit = null)
        {
            _name = name;
            _onEnter = onEnter ?? (() => { });
            _update = update ?? ((float dT) => { });
            _onExit = onExit ?? ((State s) => { });
            _complete = true;
            _isAny = false;
        }

        public void Enter()
        {
            _onEnter?.Invoke();
        }

        public void Update(float dT)
        {
            _update?.Invoke(dT);
        }

        public void Exit(IState s)
        {
            _onExit?.Invoke((State)s);
        }

        public static bool operator ==(State a, string b) {
            return a.Name == b;
        }

        public static bool operator !=(State a, string b)
        {
            return !(a.Name == b);
        }
    }
}