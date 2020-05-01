using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public sealed class SKStateMachine<T> {
    #pragma warning disable
    protected T _context;
	public event Action OnStateChanged;
	#pragma warning restore

	public SKState<T> currentState { get { return _currentState; } }
	public SKState<T> previousState { private set; get; }
	public float elapsedTimeInState = 0f;


	private Dictionary<System.Type, SKState<T>> _states = new Dictionary<System.Type, SKState<T>>();
	private SKState<T> _currentState;


	public SKStateMachine( T context, SKState<T> initialState ) {
		this._context = context;

		// setup our initial state
		AddState( initialState );
		_currentState = initialState;
		_currentState.Begin();
	}


	/// <summary>
	/// adds the state to the machine
	/// </summary>
	public void AddState( SKState<T> state ) {
		state.SetMachineAndContext( this, _context );
		_states[state.GetType()] = state;
	}


	/// <summary>
	/// ticks the state machine with the provided delta time
	/// </summary>
	public void Update( float deltaTime ) {
		elapsedTimeInState += deltaTime;
		_currentState.Reason();
		_currentState.Update( deltaTime );
	}


	/// <summary>
	/// changes the current state
	/// </summary>
	public R ChangeState<R>() where R : SKState<T> {
		// avoid changing to the same state
		var newType = typeof( R );
		if( _currentState.GetType() == newType )
			return _currentState as R;

		// only call end if we have a currentState
		if( _currentState != null )
			_currentState.End();

		#if UNITY_EDITOR
		// do a sanity check while in the editor to ensure we have the given state in our state list
		if( !_states.ContainsKey( newType ) )
		{
			var error = GetType() + ": state " + newType + " does not exist. Did you forget to add it by calling AddState?";
			Debug.LogError( error );
			throw new Exception( error );
		}
		#endif

		// swap states and call begin
		previousState = _currentState;
		_currentState = _states[newType];
		_currentState.Begin();
		elapsedTimeInState = 0f;

		// fire the changed event if we have a listener
		if( OnStateChanged != null )
			OnStateChanged();

		return _currentState as R;
	}
}
