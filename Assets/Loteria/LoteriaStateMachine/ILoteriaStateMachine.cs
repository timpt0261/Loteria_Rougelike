

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface ILoteriaGameState
{
	public void OnEnterState();
	public void OnUpdate();
	public void OnExitState();

}

public class LoteriaGameStateMachine : MonoBehaviour
{
	public List<ILoteriaGameState> loteriaGameStates = new();

	public ILoteriaGameState prevGameState = null;
	public ILoteriaGameState currentGameState;

	public void ChangeState(ILoteriaGameState newGameState)
	{
		if (currentGameState != null)
		{
			currentGameState.OnExitState();
		}

		currentGameState = newGameState;
		currentGameState.OnEnterState();
	}
}