using UnityEngine;
using System.Collections;

//=============================================================================
//  TrafficLightBase
//  by Healthbar Games (http://healthbargames.pl)
//  author: Mariusz Skowroński
//
//  Represents a single traffic light module.
//  This abstract class must be inherited by a class that implements the
//  OnLightStateChanged() callback.
//  The callback is called after every state change in the traffic light module.
//  Its implementation in a subclass should enable / disable the corresponding
//  lights (red, yellow and green).
//=============================================================================

namespace HealthbarGames
{
    public abstract class TrafficLightBase : MonoBehaviour
    {
        // traffic light states
        public enum State { Blank, Stop, PrepareToGo, Go, PrepareToStop, YellowBlink };

        // traffic light phase that set current light state
        private TrafficLightPhase mParentPhase = null;

        // called when light state has changed - must be implemented in nested classes with code for turning on/off proper light colors (red, yellow, green)
        public abstract void OnLightStateChanged(bool redLight, bool yellowLight, bool greenLight);


        void Start()
        {
            if (mParentPhase == null)
                OnLightStateChanged(false, false, false);
        }

        // Change current lights state
        public void ChangeLightState(bool redLight, bool yellowLight, bool greenLight, TrafficLightPhase parentPhase)
        {
            mParentPhase = parentPhase;
            OnLightStateChanged(redLight, yellowLight, greenLight);
        }

        // Get current light state (taken from traffic light phase that caused current light state)
        public State GetState()
        {
            if (mParentPhase != null)
                return mParentPhase.GetState();
            else
                return TrafficLightBase.State.Blank;
        }
    }
}
