using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool pathOn = false;

    public InputActionAsset playerInput;

    private InputAction pathAction;

    public GameObject MessageIn;

    public GameObject MessageOut;

    public GameObject MessagePrefab;

    public GameObject rightController;

    void Start()
    {
        pathAction = playerInput.FindActionMap("XRI LeftHand Interaction").FindAction("PathSwitch");
        pathAction.Enable();

        MessageIn.SetActive(false);
        MessageOut.SetActive(false);
    }

    void Update()
    {
        if(pathAction.triggered)
        {
            pathOn = !pathOn;
        }
        
        // Vector3 cameraPosition = Camera.main.transform.position;
        // Vector3 forwardDirection = Camera.main.transform.forward;
        
        // // Calculate the desired position with the height offset
        // Vector3 desiredPosition = cameraPosition + forwardDirection * 0.5f;
        // desiredPosition.y -= 0.1f;

        // // set the message position
        // MessageIn.transform.position = desiredPosition + new Vector3(0.1f, 0, 0);
        // MessageOut.transform.position = desiredPosition + new Vector3(-0.1f, 0, 0);

        

        // // Calculate the direction to face the camera
        // Vector3 lookDirection = cameraPosition - MessageIn.transform.position;
        // lookDirection.y = 0; // Keep the rotation only around the Y-axis

        // // Calculate the rotation to face the camera with an upward tilt
        // Quaternion targetRotation = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(5f, 180, 0);
        // MessageIn.transform.rotation = targetRotation;
        // MessageIn.transform.rotation = Quaternion.Euler(MessageIn.transform.rotation.eulerAngles.x, MessageIn.transform.rotation.eulerAngles.y, 0);

        // // Calculate the direction to face the camera
        // lookDirection = cameraPosition - MessageOut.transform.position;
        // lookDirection.y = 0; // Keep the rotation only around the Y-axis
        // Quaternion targetRotation2 = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(5f, 180, 0);
        // MessageOut.transform.rotation = targetRotation2;
        // MessageOut.transform.rotation = Quaternion.Euler(MessageOut.transform.rotation.eulerAngles.x, MessageOut.transform.rotation.eulerAngles.y, 0);

        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 forwardDirection = Camera.main.transform.forward;
        Vector3 rightDirection = Camera.main.transform.right; // Use the right direction for lateral spacing

        // Calculate the desired position with the height offset
        Vector3 desiredPosition = cameraPosition + forwardDirection * 0.5f;
        desiredPosition.y -= 0.1f;  // Apply height offset

        // Set the positions for the panels with lateral spacing
        float panelSpacing = 0.1f; // Adjust this value for the desired spacing between panels

        MessageIn.transform.position = desiredPosition - rightDirection * panelSpacing;
        MessageOut.transform.position = desiredPosition + rightDirection * panelSpacing;
        MessageIn.transform.position = new Vector3(MessageIn.transform.position.x, Camera.main.transform.position.y-0.1f, MessageIn.transform.position.z);
        MessageOut.transform.position = new Vector3(MessageOut.transform.position.x, Camera.main.transform.position.y-0.1f, MessageOut.transform.position.z);

        // Calculate the rotation to face the camera, ignoring roll (Z-axis rotation)
        Vector3 flatForward = new Vector3(forwardDirection.x, 0, forwardDirection.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(flatForward) * Quaternion.Euler(0, 0, 0);

        // Apply the same rotation to both panels to keep them aligned
        MessageIn.transform.rotation = targetRotation;
        MessageOut.transform.rotation = targetRotation;

        // Ensure that the panels remain upright and have no tilt
        MessageIn.transform.rotation = Quaternion.Euler(0, MessageIn.transform.rotation.eulerAngles.y, 0);
        MessageOut.transform.rotation = Quaternion.Euler(0, MessageOut.transform.rotation.eulerAngles.y, 0);


        
    }

    // show a UI panel to indicate the current edge/node message signal
    public void ShowMessage(List<string> messages, bool isInput)
    {
        GameObject messagePanel = isInput ? MessageIn : MessageOut;
        foreach (Transform child in messagePanel.transform.Find("Panel/ScrollView/Viewport/Content"))
        {
            Destroy(child.gameObject);
        }
        foreach (string message in messages)
        {
            GameObject messageText = Instantiate(MessagePrefab, messagePanel.transform.Find("Panel/ScrollView/Viewport/Content"));
            messageText.transform.Find("Text").GetComponent<Text>().text = message;
        }
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
        foreach (string message in messages)
        {
            if (!existingMessages.Contains(message))
            {
                GameObject messageText = Instantiate(MessagePrefab, contentTransform);
                messageText.transform.Find("Text").GetComponent<Text>().text = message;

                messageText.transform.SetAsFirstSibling(); // Ensure new messages are displayed at the top
            }
        }
    }


}
