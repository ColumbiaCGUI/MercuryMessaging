# MercuryMessaging 

![Basic scene layout](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Images/General/MercuryCollage2.png)

The *Mercury Messaging* toolkit is a new way to handle cross-component communication in the Unity
  game engine. It integrates seamlessly with the Unity Editor, and is both
 robust and expandable. 
 
*It has been tested in Unity 2022 up until 2022.3.18f1. This is the recommended version of Unity to use with Mercury! Networking features have been tested and work for PUN and Fusion in LTS 2022.*
 
The toolkit contains the *Mercury* messaging framework, which is a messaging
  and organizational framework built around the *Mercury Protocol*. 

Unity organizes its rendered scene objects
(known in Unity as
  [GameObjects](https://docs.unity3d.com/ScriptReference/GameObject.html))
using a standard scene graph (known in Unity as the
  [Scene Hierarchy](https://docs.unity3d.com/Manual/Hierarchy.html)).
While Unity is very powerful,
it is fairly difficult to achieve nonspatial communication between
scriptable components of GameObjects (in Unity, known as
  [MonoBehaviours](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html))
.

Consider a visualization in a basketball game where you connect the ball to *each* 
player on a court with individual lines. There will be a control script on each
  line in the visualization.

Normally, to disable a GameObject in Unity, you invoke the
  [SetActive](https://docs.unity3d.com/ScriptReference/GameObject.SetActive.html)
  method.
This will disable the GameObject and its children in the scene hierarchy.
However, in our example, to disable the entire visualization, you would need to
  disable the endpoint spheres and the line objects.
In a script, you would need to get a handle to the GameObjects, and invoke
  SetActive on each of them individually.  


In this simple example, the Mercury messaging toolkit makes it easy to achieve this.

You first drop an *MmRelayNode* (MercuryMessaging Relay Node) and an *MmResponder* (MercuryMessaging
  Responder) onto each of the related GameObjects in the visualization.

Each *MmRelayNode* has a *MmRoutingTable*.
In the line's root *MmRelayNode*, you'll drag and drop the related components:
  the endpoint-spheres and line.

In your line control script, you invoke the following method:

```
GetComponent<MmRelayNode>().MmInvoke(MmMethod.SetActive, true,
    new MmControlBlock(MmLevelFilterHelper.Default, MmActiveFilter.All,
    default(MmSelectedFilter), MmNetworkFilter.Local));
```

This will trigger a special SetActive message on each of the objects involved
  in the visualization.

Done!

## IEEE ISMAR 2024

A library of debugging features and UI for Mercury was presented at IEEE ISMAR 2024 with the title "An XR GUI for Visualizing Messages in ECS Architectures". 

The extended abstract for this demo will be available soon in the Adjunct Proceedings of IEEE ISMAR 2024 in IEEE Xplore.

## CHI 2018

*Mercury* was presented at CHI 2018. The paper is available online at the ACM Digital Library.

Carmine Elvezio, Mengu Sukan, and Steven Feiner. 2018. Mercury: A Messaging Framework for Modular UI Components. In Proceedings of the 2018 CHI Conference on Human Factors in Computing Systems (CHI '18). ACM, New York, NY, USA, Paper 588, 12 pages. DOI:https://doi.org/10.1145/3173574.3174162

## Downloading MercuryMessaging

GitHub
You can check-out or download the code from [GitHub](https://github.com/ColumbiaCGUI/MercuryMessaging) directly. 
If you downloaded the source from GitHub, please drag and drop the
        root folder of MercuryMessaging, *MercuryMessaging* into the Assets folder of your
        project.

## Getting Started

Once you have the toolkit installed, you'll probably want to check out a
tutorial.

Please see our tutorials page at the site: [MercuryMessaging Tutorials](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Tutorials).

## Documentation

Complete Documentation for the toolkit can be found on the toolkit's
[documentation page](https://columbiacgui.github.io/MercuryMessaging/).

## FAQ
### Q. Does the toolkit work in Unity versions earlier than LTS 2022?

A. Core functions have been tested in Unity 2020 up until 2020.3.21f1, Unity 2019 up until 2019.2.17f1, Unity 2018 up until 2018.3.13f1, Unity 2017 up until 2017.4f1, and 5.6. 

Updated networking functionality with Photon Fusion, which has only been tested in LTS 2022, *may not work* in any of these older versions. PUN works properly for these earlier versions.

### Q. What is Unity?

A. Unity is a game engine. Please see here:
[Unity](https://unity3d.com/).

### Q. Can I use the toolkit with Unreal, CryEngine, etc.

A. As much as we like those engines, we originally built the toolkit to support us in our
work in our lab, where we use Unity.


# Acknowledgments

Funded in part by National Science Foundation Grant IIS-1514429 and CMMI-2037101. 
Any opinions, findings and conclusions or recommendations expressed in this material are those of the authors and do not necessarily reflect the views of the National Science Foundation.


