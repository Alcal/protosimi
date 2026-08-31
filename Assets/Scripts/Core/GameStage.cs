using System;

namespace ManosLimpias.Core
{
    [Serializable]
    public abstract class GameStage
    {
        protected IGameFlowServices Services { get; private set; }
        public bool IsEntered { get; private set; }

        public abstract string Id { get; }

        public void Initialize(IGameFlowServices services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public void Enter()
        {
            if (IsEntered) return;
            IsEntered = true;
            OnEnter();
        }

        public void Tick(float deltaTime)
        {
            if (IsEntered)
                OnTick(deltaTime);
        }

        public void Exit()
        {
            if (!IsEntered) return;
            OnExit();
            IsEntered = false;
        }

        public abstract GameStage CreateRuntime();

        protected virtual void OnEnter() { }
        protected virtual void OnTick(float deltaTime) { }
        protected virtual void OnExit() { }
    }
}
