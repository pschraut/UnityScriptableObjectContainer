# ScriptableObject Container for Unity

The ScriptableObject Container package provides the ability to work with ScriptableObjects in a similar way how you work with Components and GameObjects.

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

You can of course use ScriptableObject Container without giving me credit.
I'm a big fan of giving credit where credit is due though :)

# Examples
TODO

