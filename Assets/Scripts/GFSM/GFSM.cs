using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Playables;
using UnityEngine.Windows;

/**
 * @author Gabe Labadie
 * @date 2025-09-04
 * @brief Gabe's Finite State Machine (GFSM)
 * 
 * My first attempt at writing a truly drag-and-drop reusable library.
 * 
 * This is a part of a larger exercise to refine my programming skills by
 * rethinking every habit, pattern, and codesmell that I've picked up spending
 * half of my life programming.
 *
 */
namespace Gabadie.GFSM
{
    public class FSM<S> where S : IState
    {
        private S _currentState;
        public S State => _currentState;

        Dictionary<string, S> _states = new();
        List<Transition<S>> _transitions = new();

        /// <summary>
        /// Runs the current state and checks for transitions.
        /// </summary>
        /// <param name="deltaTime">Time since last frame.</param>
        /// <returns>Whether a transition has occured or not.</returns>
        public bool Poll(float deltaTime)
        {
            if (State != null)
                State.Update(deltaTime);

            foreach (Transition<S> t in _transitions)
            {
                if (CanTransition(t))
                {
                    if (t.fromAny)
                        Debug.Log(State.Name + " (ANY) -> " + t.Target.Name);
                    else
                        Debug.Log(State.Name + " -> " + t.Target.Name);

                    DoTransition(t.Target);
                    return true;
                }
            }

            return false;
        }
        
        public S GetState(string name)
        {
            S res;
            if (_states.TryGetValue(name, out res))
            {
                return res;
            }

            throw new Exception($"State {name} not found in FSM.");
        }

        public void AddState(S state)
        {
            _states.Add(state.Name, state);
        }

        public void AddTransition(Transition<S> transition)
        {
            _transitions.Add(transition);
        }

        public void AddTransitions(params Transition<S>[] transitions)
        {
            _transitions.AddRange(transitions);
        }

        public void Interrupt(S state)
        {
            DoTransition(state);
        }

        /// <summary>
        /// Checks the conditions of a transition and runs it if successful.
        /// </summary>
        /// <param name="t">The transition to be checked.</param>
        /// <returns>
        /// True if transition successful.
        /// False if transition unsuccessful.
        /// </returns>
        public bool CanTransition(Transition<S> t)
        {
            if (!State.Complete) return false;
            if (t.fromAny && State.Equals(t.Target)) return false;
            if (!t.fromAny && !State.Equals(t.Source)) return false;

            if (t.conditions.All(p => p()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Forces the FSM to target state, regardless of conditions.
        /// </summary>
        /// <param name="target">State the FSM will end in.</param>
        void DoTransition(S target)
        {
            _currentState.Exit(target);
            target.Enter();
            _currentState = target;
        }

        public S this[string index]
        {
            get => GetState(index);
        }
    }

    public class FSM : FSM<State> { }
}
