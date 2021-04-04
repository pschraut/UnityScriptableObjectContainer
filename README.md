# ScriptableObject Container for Unity

The [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html) type in Unity is a very powerful concept that has many different applications.
It lacks one feature that'd skyrocket its usefulness for me and that's being able
to add "Components" to it. 
Unity allows to add "Components" to a ScriptableObject asset through code, but they didn't expose functionality to do it through the Inspector.

The ScriptableObject Container package attempts to solve this.
It allows to work with ScriptableObjects in a similar way how you work with Components and GameObjects.

You add a ScriptableObject to a ```ScriptableObjectContainer``` via the Inspector in a similar way how you add a Component to a GameObject.

On the scripting side, you get the ScriptableObject from the ```ScriptableObjectContainer``` in a similar way how you get a Component from a GameObject.

| ScriptableObjectContainer  |     GameObject      |
|----------|---------------|
| ```ScriptableObject GetObject(Type type)``` | ```Component GetComponent(Type type)``` |
| ```void GetObjects(Type type, List<T> results)``` | ```void GetComponents(Type type, List<T> results)``` |
| ```T GetObject<T>()``` | ```T GetComponent<T>``` |
| ```void GetObjects<T>(List<T> results)``` | ```void GetComponents(List<T> results)``` |

You can think of a ScriptableObjectContainer as "GameObject" and its sub-assets (or objects) would be the Components on a GameObject.

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

# Test Runner integration

The ScriptableObjectContainer package comes with several tests that run in 
[Unity's Test Runner](https://docs.unity3d.com/Packages/com.unity.test-framework@latest).

The tests can be enabled through the 
```SCRIPTABLEOBJECTCONTAINER_ENABLE_TESTS``` scripting define symbol.
Add this scripting define symbol to the Player Settings and they appear in
Unity's Test Runner.

Additionally to the new tests in the Test Runner window,
it adds various context menu items to create test assets,
which is the reason why it's disabled by default, basically 
to avoid cluttering your project with things you most likely don't need.


# Examples

## CreateSubAssetMenuAttribute

A ScriptableObjectContainer shows an "Add Object" button in the Inspector,
much like a GameObject shows a "Add Component" button, which allows to add
objects derived from ScriptableObject to the container.

In order to add a ScriptableObject to the "Add Object" menu, you need to
add the ```CreateSubAssetMenuAttribute``` to the ScriptableObject type.
```CSharp
[CreateSubAssetMenu(menuName = "Fruit")]
class Fruit : ScriptableObject
{
    // ...
}
```

## DisallowMultipleSubAssetAttribute

If you want to prevent to add the same ScriptableObject type (or subtype)
more than once to the same container, you can use the
```DisallowMultipleSubAssetAttribute```.

This works similar to how you use Unity's
 [DisallowMultipleComponentAttribute](https://docs.unity3d.com/ScriptReference/DisallowMultipleComponent.html)
to prevent a MonoBehaviour of same type (or subtype) to be added more than once to a GameObject.
```CSharp
[DisallowMultipleSubAsset]
class Fruit : ScriptableObject
{
    // ...
}
```

## SubAssetOwnerAttribute

If you need a reference to the ScriptableObjectContainer inside your ScriptableObject
sub-asset, you can use the ```SubAssetOwnerAttribute``` for the system to automatically
setup the reference for you. The code that sets up references runs in the editor only,
thus there are no runtime penalties for using it.
```CSharp
class Fruit : ScriptableObject
{
    [SubAssetOwner]
    [SerializeField] ScriptableObjectContainer m_Container;
}
```
If you know that a sub-asset lives inside a specific container type only,
you can also use that container type.
```CSharp
class Fruit : ScriptableObject
{
    [SubAssetOwner]
    [SerializeField] Basket m_Container; // The Basket type must inherit from ScriptableObjectContainer
}
```

## SubAssetToggleAttribute

Unity does not support the concept of enabling and disabling a ScriptableObject,
but I often found that I want to support this in whatever specific use-case I have.
Using the ```SubAssetToggleAttribute``` on a ```bool``` field causes the
ScriptableObjectContainer editor to display a toggle (checkbox) like you can find
in Components on a GameObject.
```CSharp
class Fruit : ScriptableObject
{
    [SubAssetToggle]
    [SerializeField] bool m_IsEnabled;
}
```
It's worth to note that you can't ```m_Enabled``` as field name, since
it conflicts with a field that Unity implements too, but seemingly Unity isn't using it.
