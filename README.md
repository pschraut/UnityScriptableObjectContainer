# ScriptableObject Container for Unity

The ScriptableObject in Unity is a very powerful concept that has many different applications.
It lacks one feature that'd skyrocket its usefulness for me and that's being able
to add "Components" to it. 
Unity allows to add "Components" through code, but they didn't expose this functionality to the Inspector.

The ScriptableObject Container package attempts to solve this.
It allows to work with ScriptableObjects in a similar way how you work with Components and GameObjects.

You add a ScriptableObject to a ```ScriptableObjectContainer``` via the Inspector in a similar way how you add a Component to a GameObject.

On the scripting side, you get a ScriptableObject from the ```ScriptableObjectContainer``` in a similar way how you get a Component from a GameObject.

| ScriptableObjectContainer  |     GameObject      |
|----------|---------------|
| ```ScriptableObject GetObject(Type type)``` | ```Component GetComponent(Type type)``` |
| ```void GetObjects(Type type, List<T> results)``` | ```void GetComponents(Type type, List<T> results)``` |
| ```T GetObject<T>()``` | ```T GetComponent<T>``` |
| ```void GetObjects<T>(List<T> results)``` | ```void GetComponents(List<T> results)``` |

# Installation

As of Unity 2019.3, Unity supports to add packages from git through the Package Manager window. 
In Unity's Package Manager, choose "Add package from git URL" and insert one of the Package URL's you can find below.

## Package URL's

| Version  |     Link      |
|----------|---------------|
| 1.0.0 | https://github.com/pschraut/UnityScriptableObjectContainer.git#1.0.0 |

# Credits

If this package is useful to you, please mention my name in your credits screen.
Something like "ScriptableObject Container by Peter Schraut" or "Thanks to Peter Schraut" would be very much appreciated.

# How it works

The package introduces the type ```ScriptableObjectContainer``` that derives from ScriptableObject.

A custom inspector implements the magic that allows to add a ScriptableObject as sub-asset, to remove such sub-asset as 
well as to change properties of such sub-asset through the Inspector. 
The ScriptableObjectContainer Inspector attempts to mimic the look and feel of Unity's built-in Inspector when working with Components.

The ScriptableObjectContainer itself is rather light-weight. It contains an array with references to its sub-assets.
This allows you to retrieve these sub-assets through code, similar how you work with the GameObject.GetComponent and GameObject.GetComponents methods.

Beside the sub-assets array, the ScriptableObjectContainer does not contain much more code
that's required in a build. It implements OnValidate when running in the Unity Editor to keep the sub-asset array synced, that's it.


# Examples
TODO

