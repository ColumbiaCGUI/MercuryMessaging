using UnityEngine;
using System.Collections;

//=============================================================================
//  HaloTrafficLight
//  by Healthbar Games (http://healthbargames.pl)
//  author: Mariusz Skowroński
//
//  More advanced implementation of TrafficLight
//  For each of three light colors (red, yellow and green) it uses
//  one mesh renderer, one object with halo effect attached and
//  one real light (Point, Spot or any other Unity's light).
//  To visualize the states of lights (on / off) it requires two materials:
//  - one with a texture for the lights turned off
//  - and one with a texture for the lights turned on.
//  You can use (like in demo scene) two different materials with single,
//  common texture for light states.
//=============================================================================

namespace HealthbarGames
{
    public class RealTrafficLight : TrafficLightBase
    {
        public Renderer RedRenderer;
        public GameObject RedHalo;
        public Light RedLight;

        public Renderer YellowRenderer;
        public GameObject YellowHalo;
        public Light YellowLight;

        public Renderer GreenRenderer;
        public GameObject GreenHalo;
        public Light GreenLight;

        public Material LightsOnMat;
        public Material LightsOffMat;

        private bool mInitialized = false;

        void Awake()
        {
            if (   (RedRenderer != null || RedHalo != null || RedLight != null)
                && (YellowRenderer != null || YellowHalo != null || YellowLight != null)
                && (GreenRenderer != null || GreenHalo != null || GreenLight != null)
                )
            {
                mInitialized = true;
            }
            else
            {
                mInitialized = false;
                Debug.LogError("Some variables haven't been assigned correctly for HaloTrafficLight script.", this);
            }
        }

        // implementation of the callback from TrafficLight - called when lights state has changed
        public override void OnLightStateChanged(bool redLightState, bool yellowLightState, bool greenLightState)
        {
            if (!mInitialized)
                return;

            if (RedHalo != null)
                RedHalo.SetActive(redLightState);

            if (RedRenderer != null)
                RedRenderer.material = (redLightState) ? LightsOnMat : LightsOffMat;

            if (RedLight != null)
                RedLight.enabled = redLightState;

            if (YellowHalo != null)
                YellowHalo.SetActive(yellowLightState);

            if (YellowRenderer != null)
                YellowRenderer.material = (yellowLightState) ? LightsOnMat : LightsOffMat;

            if (YellowLight != null)
                YellowLight.enabled = yellowLightState;

            if (GreenHalo != null)
                GreenHalo.SetActive(greenLightState);

            if (GreenRenderer != null)
                GreenRenderer.material = (greenLightState) ? LightsOnMat : LightsOffMat;

            if (GreenLight != null)
                GreenLight.enabled = greenLightState;
        }
    }
}
