using System;

namespace Gabadie.GFSM
{
    public struct Transition<S> where S : IState
    {
        public S Source { get; private set; }
        public S Target { get; private set; }

        public bool fromAny;

        public Func<bool>[] conditions;

        public Transition(S _source, S _target, params Func<bool>[] _conditions)
        {
            Source = _source;
            Target = _target;
            conditions = _conditions;
            fromAny = false;
        }

        public Transition(S _target, params Func<bool>[] _conditions)
        {
            //Source = Target doesn't actually do anything, we just need a value
            Source = default;
            Target = _target;
            conditions = _conditions;
            fromAny = true;
        }
    }
}
