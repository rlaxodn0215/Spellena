using UnityEngine;

namespace FSM
{
    public class StateMachine : MonoBehaviour
    {
        protected BaseState currentState;

        protected void Start()
        {
            currentState = GetInitalState();
            if (currentState != null)
                currentState.Enter();
        }

        protected virtual void Update()
        {
            if (currentState != null)
                currentState.Update();
        }

        protected virtual void FixedUpdate()
        {
            if (currentState != null)
                currentState.FixedUpdate();
        }

        public void ChangeState(BaseState newState)
        {
            currentState.Exit();

            currentState = newState;
            currentState.Enter();
        }

        protected virtual BaseState GetInitalState()
        {
            return null;
        }

    }
}