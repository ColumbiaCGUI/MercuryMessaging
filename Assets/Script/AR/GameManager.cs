using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Vuplex.WebView;

public class GameManager : MonoBehaviour
{
    public bool pathOn = false;

    private bool vuplexOn = false;

    public InputActionAsset playerInput;

    private InputAction pathAction;

    private InputAction joyStickClick;

    private int triggerTimes = 0;

    public GameObject Vuplex;

    public GameObject MessageIn;

    public GameObject MessageOut;

    public GameObject MessagePrefab;

    public GameObject rightController;

    float panelSpacing = 0.1f;

    void Awake() 
    {
        #if UNITY_STANDALONE || UNITY_EDITOR
            // On Windows and macOS, change the User-Agent to mobile:
            Web.SetUserAgent(true);
        #elif UNITY_IOS
            // On iOS, change the User-Agent to desktop:
            Web.SetUserAgent(false);
        #else
            // Otherwise, change the User-Agent to a recent version of FireFox (Google blocks older versions).
            var firefox100ReleaseDate = DateTime.Parse("2022-05-03");
            var currentVersion = 100 + ((DateTime.Now.Year - firefox100ReleaseDate.Year) * 12) + DateTime.Now.Month - firefox100ReleaseDate.Month;
            Web.SetUserAgent($"Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:{currentVersion}.0) Gecko/20100101 Firefox/{currentVersion}.0");
        #endif

    }
    void Start()
    {
        pathAction = playerInput.FindActionMap("XRI LeftHand Interaction").FindAction("PathSwitch");
        pathAction.Enable();

        joyStickClick = playerInput.FindActionMap("XRI LeftHand Interaction").FindAction("JoyStickPress");
        joyStickClick.Enable();

        MessageIn.SetActive(false);
        MessageOut.SetActive(false);

        

        // instantiate 10 messages for each panel using message prefab
        for (int i = 0; i < 10; i++)
        {
            // the messages do not have any content yet
            GameObject messageTextIn = Instantiate(MessagePrefab, MessageIn.transform.Find("Panel/ScrollView/Viewport/Content"));
            GameObject messageTextOut = Instantiate(MessagePrefab, MessageOut.transform.Find("Panel/ScrollView/Viewport/Content"));
            
        }
        
    }

    void Update()
    {
        if(pathAction.triggered)
        {
            // pathOn = !pathOn;
            triggerTimes= (triggerTimes + 1)%3;
        }

        if(triggerTimes == 0)
        {
            vuplexOn = false;
            pathOn=false;
            Vuplex.SetActive(false);
            // Vuplex.GetComponent<Canvas>().enabled = false;
        }
        else if(triggerTimes == 1)
        {
            vuplexOn = false;
            pathOn=true;
            Vuplex.SetActive(false);
            // Vuplex.GetComponent<Canvas>().enabled = false;
        }
        else if(triggerTimes == 2)
        {
            if(!vuplexOn)
            {
                ResetVuplex();
                vuplexOn = true;
            }
            pathOn=true;
            Vuplex.SetActive(true);
            // Vuplex.GetComponent<Canvas>().enabled = true;
        }
        
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 forwardDirection = Camera.main.transform.forward;
        Vector3 rightDirection = Camera.main.transform.right; // Use the right direction for lateral spacing

        // Calculate the desired position with the height offset
        Vector3 desiredPosition = cameraPosition + forwardDirection * 0.5f;
        desiredPosition.y -= 0.1f;  // Apply height offset

        // Set the positions for the panels with lateral spacing
        // float panelSpacing = 0.1f; // Adjust this value for the desired spacing between panels
        

        // Vector3 vuplexPosition = cameraPosition + forwardDirection * 1.8f;
        // // update the 3 panels to be in front of the camera
        // Vuplex.transform.position = vuplexPosition;
        // Vuplex.transform.position = new Vector3(Vuplex.transform.position.x, Camera.main.transform.position.y-0.1f, Vuplex.transform.position.z);

        MessageIn.transform.position = desiredPosition - rightDirection * panelSpacing;
        MessageOut.transform.position = desiredPosition + rightDirection * panelSpacing;
        MessageIn.transform.position = new Vector3(MessageIn.transform.position.x, Camera.main.transform.position.y-0.1f, MessageIn.transform.position.z);
        MessageOut.transform.position = new Vector3(MessageOut.transform.position.x, Camera.main.transform.position.y-0.1f, MessageOut.transform.position.z);

        // Calculate the rotation to face the camera, ignoring roll (Z-axis rotation)
        Vector3 flatForward = new Vector3(forwardDirection.x, 0, forwardDirection.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(flatForward) * Quaternion.Euler(0, 0, 0);

        // // Apply the same rotation on Vuplex menu
        // Vuplex.transform.rotation = targetRotation;

        // Apply the same rotation to both panels to keep them aligned
        MessageIn.transform.rotation = targetRotation;
        MessageOut.transform.rotation = targetRotation;

        // ensure the panels of vuplex remain upright and no tilt
        // Vuplex.transform.rotation = Quaternion.Euler(0, Vuplex.transform.rotation.eulerAngles.y, 0);

        // Ensure that the panels remain upright and have no tilt
        MessageIn.transform.rotation = Quaternion.Euler(0, MessageIn.transform.rotation.eulerAngles.y, 0);
        MessageOut.transform.rotation = Quaternion.Euler(0, MessageOut.transform.rotation.eulerAngles.y, 0);
        
    }


    private void ResetVuplex()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 forwardDirection = Camera.main.transform.forward;

        Vector3 vuplexPosition = cameraPosition + forwardDirection * 2.0f;
        // update the 3 panels to be in front of the camera
        Vuplex.transform.position = vuplexPosition;
        Vuplex.transform.position = new Vector3(Vuplex.transform.position.x, Camera.main.transform.position.y-0.1f, Vuplex.transform.position.z);

        // Calculate the rotation to face the camera, ignoring roll (Z-axis rotation)
        Vector3 flatForward = new Vector3(forwardDirection.x, 0, forwardDirection.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(flatForward) * Quaternion.Euler(0, 0, 0);

        // Apply the same rotation on Vuplex menu
        Vuplex.transform.rotation = targetRotation;

        // ensure the panels of vuplex remain upright and no tilt
        Vuplex.transform.rotation = Quaternion.Euler(0, Vuplex.transform.rotation.eulerAngles.y, 0);
    }

    // show a UI panel to indicate the current edge/node message signal
    public void ShowMessage(List<string> messages, bool isInput)
    {
        GameObject messagePanel = isInput ? MessageIn : MessageOut;
        foreach (Transform child in messagePanel.transform.Find("Panel/ScrollView/Viewport/Content"))
        {
            child.transform.Find("Text").GetComponent<Text>().text = "";
        }

        for(int i = 0; i < messages.Count; i++)
        {
            messagePanel.transform.Find("Panel/ScrollView/Viewport/Content").GetChild(i).Find("Text").GetComponent<Text>().text = messages[i];
        }
        // {
        //     // GameObject messageText = Instantiate(MessagePrefab, messagePanel.transform.Find("Panel/ScrollView/Viewport/Content"));
        //     // messageText.transform.Find("Text").GetComponent<Text>().text = message;
        // }
    }

    public void UpdateCurrentMessage(List<string> messages, bool isInput)
    {
        GameObject messagePanel = isInput ? MessageIn : MessageOut;
        Transform contentTransform = messagePanel.transform.Find("Panel/ScrollView/Viewport/Content");

        // Get the existing messages in the panel
        List<string> existingMessages = new List<string>();
        foreach (Transform child in contentTransform)
        {
            string existingMessage = child.Find("Text").GetComponent<Text>().text;
            existingMessages.Add(existingMessage);
        }

        // Only instantiate new messages that are not already displayed
        // foreach (string message in messages)
        // {
        //     if (!existingMessages.Contains(message))
        //     {
        //         GameObject messageText = Instantiate(MessagePrefab, contentTransform);
        //         messageText.transform.Find("Text").GetComponent<Text>().text = message;

        //         messageText.transform.SetAsFirstSibling(); // Ensure new messages are displayed at the top
        //     }
        // }

        // instead of instantiating new messages, update the existing messages
        for(int i = 0; i < messages.Count; i++)
        {
            if(!existingMessages.Contains(messages[i]))
            {
                contentTransform.GetChild(i).Find("Text").GetComponent<Text>().text = messages[i];
            }
            // contentTransform.GetChild(i).Find("Text").GetComponent<Text>().text = messages[i];
        }
    }
}
