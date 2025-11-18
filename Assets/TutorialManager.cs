using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public List<string> tutorialText;
    public int currentTutorialIndex = 0;
    
    public TMPro.TextMeshProUGUI tutorialTextUI;
    
    void Start()
    {
        // set the first tutorial text
        tutorialTextUI.text = tutorialText[currentTutorialIndex];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void NextTutorial()
    {
        if (currentTutorialIndex < tutorialText.Count - 1)
        {
            currentTutorialIndex++;
            tutorialTextUI.text = tutorialText[currentTutorialIndex];
        }
        else if (currentTutorialIndex == tutorialText.Count - 1)
        {
            // disable the canvas self
            GetComponent<Canvas>().enabled = false;
        }
    }

    public void PreviousTutorial()
    {
        if (currentTutorialIndex > 0)
        {
            currentTutorialIndex--;
            tutorialTextUI.text = tutorialText[currentTutorialIndex];
        }
    }
    
    
}
