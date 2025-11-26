using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//=============================================================================
//  TrafficLightManager
//  by Healthbar Games (http://healthbargames.pl)
//  author: Mariusz Skowroński
//
//  Manager that controls traffic light system simulation.
//
//=============================================================================

namespace HealthbarGames
{
    public class TrafficLightManager : MonoBehaviour
    {

        #region Default phase timings
        // we use this parameters only as serialized fields in custom traffic manager's editor
        // (TrafficLightManagerEditor) so we disable warning 0414 because compiler
        // detects this as not used variables
#pragma warning disable 0414
        // default phase start time that is used to initialize new phases
        [SerializeField]
        private float DefaultPhaseStartTime = 2.0f;

        // default phase active time that is used to initialize new phases
        [SerializeField]
        private float DefaultPhaseActiveTime = 10.0f;

        // default phase end time that is used to initialize new phases
        [SerializeField]
        private float DefaultPhaseEndTime = 2.0f;
#pragma warning restore 0414
        #endregion

        // delay between end of curent phase and start of next phase
        [SerializeField]
        private float PhaseDelay = 1.0f;

        // yellow light blink frequency (x times per second)
        [SerializeField]
        private float YellowBlinkFreq = 1.0f;

        // list of all phases for this traffic light manager (phases sequence)
        [SerializeField]
        private List<TrafficLightPhase> PhaseList;

        // defined programs (Main - normal work, Malfunction - yellow light blinking)
        public enum Program { None, Main, Malfunction };

        // initial program - used when scene is started
        [SerializeField]
        private Program InitialProgram = Program.Main;

        // current program
        private Program mCurrentProgram = Program.None;

        // index of currently active phase (phase that currently goes from 'Stop' state to 'Go' state and again to 'Stop' state)
        private int mCurrentPhaseIndex;

        // currently active phase
        private TrafficLightPhase mCurrentPhase;


        void Start()
        {
            // after scene start load initial program
            ChangeProgram(InitialProgram);
        }

        // gets current program
        public Program GetProgram()
        {
            return mCurrentProgram;
        }

        // stops currently working program and loads new program
        public void ChangeProgram(Program program)
        {
            StopAllCoroutines();
            mCurrentProgram = program;
            switch (mCurrentProgram)
            {
                case Program.Main:
                    StartCoroutine(MainProgramCo());
                    break;

                case Program.Malfunction:
                    StartCoroutine(YellowBlinkProgramCo());
                    break;

                default:
                    StartCoroutine(YellowBlinkProgramCo());
                    break;
            }
        }

        // coroutine for main program
        private IEnumerator MainProgramCo()
        {
            // select first phase from list
            mCurrentPhaseIndex = 0;
            mCurrentPhase = PhaseList[0];

            // begin with all traffic lights modules set to 'Stop' state (red lights)
            SetAllPhasesTo(TrafficLightBase.State.Stop);
            while (true)
            {
                // set current phase to 'PrepareToGo' state (red and yellow lights)
                mCurrentPhase.SetState(TrafficLightBase.State.PrepareToGo);
                // wait for end of state
                yield return new WaitForSeconds(mCurrentPhase.PhaseStartTime);

                // set current phase to 'Go' state (green lights)
                mCurrentPhase.SetState(TrafficLightBase.State.Go);
                // wait for end of state
                yield return new WaitForSeconds(mCurrentPhase.PhaseActiveTime);

                // set current phase to 'PrepareToStop' state (yellow lights)
                mCurrentPhase.SetState(TrafficLightBase.State.PrepareToStop);
                // wait for end of state
                yield return new WaitForSeconds(mCurrentPhase.PhaseEndTime);

                // set current phase to 'Stop' state (red lights)
                mCurrentPhase.SetState(TrafficLightBase.State.Stop);

                // this phase has ended so make delay between phases before nex phase will be started
                yield return new WaitForSeconds(PhaseDelay);

                // calculate next phase index
                mCurrentPhaseIndex++;
                mCurrentPhaseIndex = mCurrentPhaseIndex % PhaseList.Count;

                // select next phase and continue program's loop
                mCurrentPhase = PhaseList[mCurrentPhaseIndex];
            }
        }

        // sets all phases (and coresponding traffic lights) to specific state
        private void SetAllPhasesTo(TrafficLightBase.State state)
        {
            foreach (TrafficLightPhase phase in PhaseList)
            {
                phase.SetState(state);
            }
        }


        // coroutine for malfunction (yellow light blinking) program
        private IEnumerator YellowBlinkProgramCo()
        {
            // set all phases and coresponding traffic light modules to yellow blinking state
            SetAllPhasesTo(TrafficLightBase.State.YellowBlink);
            // calculate blink delay based on blink frequency
            float blinkDelay = (YellowBlinkFreq > 0.0f) ? 1.0f / YellowBlinkFreq : 1000.0f;
            bool blinkState = false;
            while (true)
            {
                // change all yellow lights state to opposite
                blinkState = !blinkState;
                foreach (TrafficLightPhase phase in PhaseList)
                {
                    phase.YellowBlink(blinkState);
                }
                // now wait for calculated amount of time between each yellow blink
                yield return new WaitForSeconds(blinkDelay);
            }
        }
    }

}
