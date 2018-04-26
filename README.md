# MercuryMessaging 

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

[GitHub](https://github.com/ColumbiaCGUI/MercuryMessaging)
You can check-out or download the code from GitHub directly. 
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
### Q. Does the toolkit work in Unity version 5.4.x, 4.x, 3.x, and earlier?

A. The Framework was developed using Unity 5.6, and has been tested in all versions of Unity 2017 and Unity 2018.

As to earlier versions, possibly. The toolkit requires some features that were added in Unity 5. As such,
we provide no support for the toolkit in earlier versions of Unity. That said, it may work in other versions of Unity 5, but we're not sure.

### Q. What is Unity?

A. Unity is a game engine. Please see here:
[Unity](https://unity3d.com/).

### Q. Can I use the toolkit with Unreal, CryEngine, etc.

A. As much as we like those engines, we originally built the toolkit to support us in our
work in our lab, where we use Unity.

