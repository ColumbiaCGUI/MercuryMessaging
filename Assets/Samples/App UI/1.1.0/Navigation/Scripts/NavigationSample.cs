using System;
using UnityEngine;
using Unity.AppUI.Navigation;
using UnityEngine.UIElements;

namespace Unity.AppUI.Samples.Navigation
{
    public class NavigationSample : MonoBehaviour
    {
        public UIDocument uiDocument;
        
        public NavGraphViewAsset graphAsset;
        
        void Start()
        {
            var navHost = new NavHost();
            navHost.navController.SetGraph(graphAsset);
            navHost.visualController = new MyVisualController();

            var panel = new UI.Panel
            {
                scale = "large"
            };
            uiDocument.rootVisualElement.Add(panel);
            panel.StretchToParentSize();
            
            panel.Add(navHost);
            navHost.StretchToParentSize();
        }
    }
}
