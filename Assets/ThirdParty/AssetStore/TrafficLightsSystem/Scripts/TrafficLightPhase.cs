using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//=============================================================================
//  TrafficLightPhase
//  by Healthbar Games (http://healthbargames.pl)
//  author: Mariusz Skowroński
//
//  Represents single phase in traffic light system simulation.
//  One phase is a process which always:
//  1) starts with 'Stop' state (red light)
//  2) then it goes to 'PrepareToGo' state (red and yellow lights)
//  3) then it goes to 'Go' state (green light)
//  4) then it goes to 'PrepareToStop' state (yellow light)
//  5) then it goes back to 'Stop' state
//
//  Phases are attached to and being managed by TrafficLightManager which starts
//  and ends each phase in a sequence one by one.
//
//  Each phase can have many traffic light modules attached to it. All light
//  modules attached to particular phase always has same state as parent phase.
//=============================================================================

namespace HealthbarGames
{
    [System.Serializable]
    public class TrafficLightPhase
    {
        // name of the phase
        public string Name;
        // duration of red and yellow light (time between 'Stop' and 'Go' states)
        public float PhaseStartTime = 2.0f;
        // duration of green light (duration of 'Go' state)
        public float PhaseActiveTime = 10.0f;
        // duration of yellow light (time between 'Go' and 'Stop' states)
        public float PhaseEndTime = 2.0f;

        // list of attached traffic lights (all this lights will be always in the same state as this phase)
        public TrafficLightBase[] TrafficLights;

        // current state of this phase
        private TrafficLightBase.State mState = TrafficLightBase.State.Blank;

        // sets name nad timings for this phase
        public void Initialize(string name, float startTime, float activeTime, float endTime)
        {
            Name = name;
            PhaseStartTime = startTime;
            PhaseActiveTime = activeTime;
            PhaseEndTime = endTime;
            mState = TrafficLightBase.State.Blank;
        }

        // sets current state of this phase (and all attached traffic lights)
        public void SetState(TrafficLightBase.State state)
        {
            //if (mState == state)
            //    return;

            mState = state;
            switch (mState)
            {
                case TrafficLightBase.State.Blank:
                    ChangeLightState(false, false, false);
                    break;

                case TrafficLightBase.State.Stop:
                    ChangeLightState(true, false, false);
                    break;

                case TrafficLightBase.State.PrepareToGo:
                    ChangeLightState(true, true, false);
                    break;

                case TrafficLightBase.State.Go:
                    ChangeLightState(false, false, true);
                    break;

                case TrafficLightBase.State.PrepareToStop:
                    ChangeLightState(false, true, false);
                    break;

                case TrafficLightBase.State.YellowBlink:
                    ChangeLightState(false, false, false);
                    break;

                default:
                    ChangeLightState(false, false, false);
                    break;
            }
        }

        // gets current state of this phase
        public TrafficLightBase.State GetState()
        {
            return mState;
        }

        // gets duration (in seconds) for current state of this phase
        public float GetCurrentStateDuration()
        {
            switch (mState)
            {
                case TrafficLightBase.State.PrepareToGo:
                    return PhaseStartTime;

                case TrafficLightBase.State.Go:
                    return PhaseActiveTime;

                case TrafficLightBase.State.PrepareToStop:
                    return PhaseEndTime;

                default:
                    return 0.0f;
            }
        }

        // sets yellow lights state for all attached traffic lights (called during yellow light blink loop)
        public void YellowBlink(bool lightState)
        {
            foreach (TrafficLightBase tl in TrafficLights)
            {
                tl.ChangeLightState(false, lightState, false, this);
            }
        }

        // notifies all attached traffic lights about new state
        private void ChangeLightState(bool redLightState, bool yellowLightState, bool greenLightState)
        {
            foreach (TrafficLightBase tl in TrafficLights)
            {
                tl.ChangeLightState(redLightState, yellowLightState, greenLightState, this);
            }
        }
    }
}
