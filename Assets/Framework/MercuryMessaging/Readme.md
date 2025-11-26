# MercuryMessaging Toolkit

*MercuryMessaging* is a new way to handle cross-component communication in the Unity
  game engine. It integrates seamlessly with the Unity Editor, and is both
  robust and very expandable.

The toolkit contains the *MercuryMessaging* Framework, which is a messaging
  and organizational framework built around the *MercuryMessaging Protocol*. 

Unity organizes its rendered scene objects
(known in Unity as
  [GameObjects](https://docs.unity3d.com/ScriptReference/GameObject.html))
using a standard scene graph (known in Unity as the
  [Scene Hierarchy](https://docs.unity3d.com/Manual/Hierarchy.html)).
While Unity's implementation is very powerful,
it is fairly difficult to achieve non-spatial communication between
scriptable components of GameObjects (in Unity, known as
  [MonoBehaviours](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html))
.

Consider a visualization in a basketball game where you connect the ball to *each* 
player on a court with individual lines, and then
  another from the ball in the hoop. There will be a control script on each
  line in the effect.

Normally, to disable a GameObject in Unity, you invoke the
  [SetActive](https://docs.unity3d.com/ScriptReference/GameObject.SetActive.html)
  method.
This will disable the GameObject and it's children in the scene hierarchy.
However, in our example, to disable the entire effect, you would need to
  go and disable the endpoint spheres and the line objects.
In a script, you would need to get a handle to the GameObjects, and invoke
  SetActive on each of them individually.  


In this simple example, the toolkit makes it easy to achieve this.

You drop an *MmRelayNode* (MercuryMessaging Relay Node) and an *MmResponder* (MercuryMessaging
  Responder) onto each of the related GameObjects in the effect.

Each *MmRelayNode* has a *MmRoutingTable*.
In the Line's root *MmRelayNode*, you'll drag and drop the related components:
  the endpoint-spheres and line.

In your line control script, you invoke the following method:

```
GetComponent<MmRelayNode>().MmInvoke(MmMethod.SetActive, true,
    new MmControlBlock(MmLevelFilterHelper.Default, MmActiveFilter.All,
    default(MmSelectedFilter), MmNetworkFilter.Local));
```

This will trigger a special SetActive message on each of the objects involved
  in the effect.

Done!

## Downloading MercuryMessaging

To get the MercuryMessaging Toolkit, please visit any of the following sources:

[MercuryMessaging Official Website](https:www.cs.columbia.edu/cgui/MercuryMessaging)

Download the *MercuryMessaging.unitypackage* file from the website.
In Unity, in the menu bar, select **Assets->Import Package->Custom package**.
Then navigate to where you stored the package file: *MercuryMessaging.unitypackage*.
Alternatively, simply drag and drop the *MercuryMessaging.unitypackage*
  file into the Assets folder in Unity's project view.
Double click the file and select import when the **Import**
  dialogue window appears.

[Unity Asset Store](https://www.assetstore.unity3d.com/en/#!/)

You'll be able to import the package immediately after downloading.
Once it is finished downloading, double click the file and select import when
the **Import** dialogue window appears.

[GitHub](https://github.com/ColumbiaCGUI)

If you downloaded the source from GitHub, please drag and drop the
        root folder of MercuryMessaging, *MercuryMessaging* into the Assets folder of your
        project.

## Getting Started

Now that you have the toolkit installed, you'll probably want to check out a
tutorial.

Please see our tutorials page at the site: [MercuryMessaging Tutorials](https://www.cs.columbia.edu/cgui/MercuryMessaging/Tutorials).

## Documentation

Complete Documentation for the toolkit can be found on the toolkit's
[documentation page](https://www.cs.columbia.edu/cgui/MercuryMessaging/Documentation).

## FAQ
### Q. Does the toolkit work in Unity version 5.4.x, 4.x, 3.x, and earlier?

A. Don't know. The toolkit requires some features that were added in Unity 5. As such,
we provide no support for the toolkit in earlier versions of Unity.

That said, it may work in other versions of Unity 5, but we're not sure.

### Q. What is Unity?

A. Unity is a game engine. Please see here:
[Unity](https://unity3d.com/).

### Q. Can I use the toolkit with Unreal, CryEngine, etc.

A. As much as we like those engines, we built the toolkit to support us in our
work in our lab, where we do all of our work in Unity.

We want to bring the toolkit to other platforms and are looking for
collaborators in doing so. Please contact
[Professor Steven Feiner](feiner@cs.columbia.edu) of Columbia University.
