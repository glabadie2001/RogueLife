namespace Gabadie.GFSM
{
    public interface IState
    {
        public string Name { get; }

        public abstract void Enter();

        public abstract void Update(float dT);

        public abstract void Exit(IState s);

        public bool Complete { get; }

        public bool IsAny { get; }

        public static IState Any { get; }
    }
}
