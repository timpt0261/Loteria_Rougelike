using System;
using UnityEngine;

namespace Utility
{
	public abstract class StateMachine : MonoBehaviour
	{
		[field: SerializeField]
		protected State currentState;

		protected void ChangeState(State newState)
		{
			currentState.ExitState();
			currentState = newState;
			currentState.EnterState(this);
		}

	}
}




