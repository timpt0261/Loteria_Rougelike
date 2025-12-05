using System;
using UnityEngine;
namespace Utility
{
	public abstract class State
	{
		protected const int ZERO = 0;


		// timer
		protected float duration;
		protected float timeRemaining;
		protected bool timerRunning;
		protected StateMachine stateMachine;

		// event triggers
		protected static event Action OnEnterState;
		protected static event Action OnExitState;



		public State(float _duration = 0)
		{
			duration = _duration;
		}

		public virtual void EnterState(StateMachine _stateMachine)
		{
			stateMachine = _stateMachine;
			OnEnterState?.Invoke();
			duration = timeRemaining;

		}

		public virtual void UpdateState()
		{

			if (!timerRunning)
				return;

			if (timeRemaining > ZERO)
			{
				timeRemaining -= Time.deltaTime;
				return;
			}

			timeRemaining = ZERO;
			timerRunning = false;

		}


		public virtual void ExitState()
		{
			OnExitState?.Invoke();

		}
	}

}
