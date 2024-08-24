using UnityEngine;
using System.Collections;

namespace HealthbarGames
{
    class TL_Demo : MonoBehaviour
    {
        public float initialDelay = 5.0f;
        public float malfunctionMinTime = 10.0f;
        public float malfunctionMaxTime = 30.0f;
        public float malfunctionDelay = 60.0f;

        TrafficLightManager[] mTrafficManagers;
        float[] mMalfunctionTimers;

        float mStartTime = 0.0f;
        float mNextMalfunctionTime = 0.0f;
        bool mWasInitialDelay = false;

        void Start()
        {
            mStartTime = Time.timeSinceLevelLoad;
            mNextMalfunctionTime = mStartTime + initialDelay + malfunctionDelay;
            mWasInitialDelay = false;

            mTrafficManagers = FindObjectsOfType<TrafficLightManager>();
            mMalfunctionTimers = new float[mTrafficManagers.Length];

            for (int i = 0; i < mTrafficManagers.Length; i++)
            {
                mTrafficManagers[i].ChangeProgram(TrafficLightManager.Program.Malfunction);
                mMalfunctionTimers[i] = 0.0f;
            }
        }

        void Update()
        {
            if (mWasInitialDelay)
            {
                // update malfunction timers
                for (int i = 0; i < mTrafficManagers.Length; i++)
                {
                    mMalfunctionTimers[i] -= Time.deltaTime;
                    if (mMalfunctionTimers[i] < 0)
                        mMalfunctionTimers[i] = 0;
                }

                // generate malfunctions
                if (Time.timeSinceLevelLoad >= mNextMalfunctionTime)
                {
                    // get random traffic manager index
                    int index = Random.Range(0, mTrafficManagers.Length);
                    // if this traffic manager runs main program - we can set it to malfunction
                    if (mTrafficManagers[index].GetProgram() == TrafficLightManager.Program.Main)
                    {
                        // change traffic manager's program to malfunction
                        mTrafficManagers[index].ChangeProgram(TrafficLightManager.Program.Malfunction);
                        // set timer for this traffic manager to some random value between min and max allowed malfunction delay
                        mMalfunctionTimers[index] = Random.Range(malfunctionMinTime, malfunctionMaxTime);
                        // calculate when we need to make another malfunction in some traffic manager
                        mNextMalfunctionTime = Time.timeSinceLevelLoad + malfunctionDelay;
                    }
                }

                // check if we need to revert back any malfunctioning lights to main program
                for (int i = 0; i < mTrafficManagers.Length; i++)
                {
                    if (mTrafficManagers[i].GetProgram() == TrafficLightManager.Program.Malfunction && mMalfunctionTimers[i] == 0)
                    {
                        mTrafficManagers[i].ChangeProgram(TrafficLightManager.Program.Main);
                    }
                }
            }
            else
            {
                if ((Time.timeSinceLevelLoad - mStartTime) > initialDelay)
                {
                    mWasInitialDelay = true;
                    for (int i = 0; i < mTrafficManagers.Length; i++)
                    {
                        mTrafficManagers[i].ChangeProgram(TrafficLightManager.Program.Main);
                    }
                }
            }
        }
    }
}