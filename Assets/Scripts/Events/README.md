# GameFramework Events System

A comprehensive, production-ready trigger and event system for Unity 6 that empowers designers while maintaining high performance.

## 🚀 Features

### Core Architecture
- **Component-Based Design**: Modular system built on composition over inheritance
- **Interface-Driven**: Clean separation of concerns with well-defined interfaces
- **Assembly Organization**: Proper dependency management with dedicated assemblies

### Event Communication
- **Hybrid Event System**: Both UnityEvents (designer-friendly) and C# events (performance-critical)
- **ScriptableObject Channels**: Asset-based events that persist between scenes
- **Type-Safe Events**: Support for int, float, string, Vector3, GameObject, and custom types
- **Event Bus**: Global communication system for cross-scene events

### Trigger Types
- **CollisionTrigger**: Physics-based collision detection
- **ProximityTrigger**: Distance-based detection with spatial optimization
- **TimerTrigger**: Time-based triggers (countdown, interval, delayed)
- **StateTrigger**: Game state condition monitoring
- **InputTrigger**: User input detection
- **CustomTrigger**: Extensible base for new trigger types

### Action System
- **TransformActions**: Move, rotate, scale objects with easing
- **GameObjectActions**: Enable/disable, instantiate, destroy
- **EventActions**: Raise event channels
- **ComponentActions**: Enable/disable components
- **Extensible**: Easy to add new action types

### Performance Features
- **Spatial Partitioning**: Efficient collision detection for large scenes
- **Batch Processing**: Process multiple triggers per frame with configurable limits
- **Performance Monitoring**: Built-in profiling and optimization tools
- **Memory Management**: Object pooling and efficient data structures

### Designer Tools
- **Custom Editors**: Enhanced inspectors with test functionality
- **Visual Debugging**: Gizmos, debug logs, and performance displays
- **Context Menus**: Quick testing and configuration options
- **Real-time Statistics**: Live performance and usage data

## 📁 Project Structure

```
Assets/Scripts/Events/
├── Actions/                    # Trigger action components
│   ├── BaseTriggerAction.cs   # Base class for all actions
│   ├── TransformActions.cs    # Move, rotate, scale actions
│   ├── GameObjectActions.cs   # GameObject manipulation actions
│   └── EventChannelActions.cs # Event channel raising actions
├── Channels/                   # Event channel ScriptableObjects
│   ├── BaseEventChannel.cs    # Base class for event channels
│   ├── GameEvent.cs           # Simple parameterless events
│   └── TypedEventChannel.cs   # Typed event channels
├── Conditions/                 # Trigger condition components
│   ├── BaseTriggerCondition.cs # Base class for conditions
│   └── CommonConditions.cs    # Standard condition implementations
├── Editor/                     # Custom editor tools
│   ├── ReadOnlyPropertyDrawer.cs # Read-only field drawer
│   └── EventChannelEditor.cs  # Custom event channel editors
├── Listeners/                  # Event listener components
│   ├── GameEventListener.cs   # Basic event listener
│   └── TypedEventListener.cs  # Typed event listeners
├── Performance/                # Performance optimization
│   ├── TriggerManager.cs      # Central trigger optimization
│   └── PerformanceMonitor.cs  # Performance tracking
├── Triggers/                   # Trigger components
│   ├── BaseTrigger.cs         # Base class for all triggers
│   ├── CollisionTrigger.cs    # Physics collision triggers
│   ├── ProximityTrigger.cs    # Distance-based triggers
│   └── TimerTrigger.cs        # Time-based triggers
└── README.md                   # This file
```

## 🎯 Quick Start

### 1. Create Event Channels

Create event assets for communication:

```csharp
// In Project window: Create > GameFramework > Events > Game Event
// Name it "PlayerEnteredArea"
```

### 2. Add a Collision Trigger

```csharp
// Add to a GameObject with a Collider (set as Trigger)
var trigger = gameObject.AddComponent<CollisionTrigger>();
trigger.triggerEvent = CollisionTrigger.TriggerEvent.OnEnter;
trigger.requiredTag = "Player";
```

### 3. Add Actions

```csharp
// Raise an event when triggered
var eventAction = gameObject.AddComponent<RaiseGameEventAction>();
eventAction.SetGameEvent(playerEnteredAreaEvent);

// Move an object
var moveAction = gameObject.AddComponent<MoveAction>();
moveAction.SetTarget(Vector3.up * 5f);
```

### 4. Add Event Listeners

```csharp
// On another GameObject
var listener = gameObject.AddComponent<GameEventListener>();
listener.GameEvent = playerEnteredAreaEvent;
// Configure UnityEvent in inspector
```

## 🔧 Advanced Usage

### Custom Conditions

```csharp
public class HealthCondition : BaseTriggerCondition
{
    [SerializeField] private float requiredHealth = 50f;
    
    protected override bool EvaluateCondition(GameObject context)
    {
        var health = context.GetComponent<HealthComponent>();
        return health != null && health.CurrentHealth >= requiredHealth;
    }
}
```

### Custom Actions

```csharp
public class PlaySoundAction : BaseTriggerAction
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;
    
    protected override void PerformAction(GameObject context)
    {
        if (audioSource && audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }
}
```

### Event Channel Usage

```csharp
// Create typed event channel
[CreateAssetMenu(menuName = "Game/Events/Health Event")]
public class HealthEventChannel : TypedEventChannel<float> { }

// Raise events from code
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private HealthEventChannel healthChangedEvent;
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthChangedEvent.RaiseEvent(currentHealth);
    }
}

// Listen to events from code
public class HealthUI : MonoBehaviour
{
    [SerializeField] private HealthEventChannel healthChangedEvent;
    
    private void OnEnable()
    {
        healthChangedEvent.OnEventRaised += UpdateHealthBar;
    }
    
    private void OnDisable()
    {
        healthChangedEvent.OnEventRaised -= UpdateHealthBar;
    }
    
    private void UpdateHealthBar(float health)
    {
        // Update UI
    }
}
```

### Performance Optimization

```csharp
// Add to scene for automatic optimization
var triggerManager = new GameObject("Trigger Manager");
triggerManager.AddComponent<TriggerManager>();

// Configure spatial partitioning
var manager = TriggerManager.Instance;
manager.SetSpatialPartitioning(true);
manager.SetMaxTriggersPerFrame(50);
manager.SetProximityUpdateInterval(0.1f);
```

## 📊 Performance Guidelines

### Trigger Limits
- **Small Scenes**: Up to 100 triggers without optimization
- **Medium Scenes**: 100-500 triggers with TriggerManager
- **Large Scenes**: 500+ triggers with spatial partitioning

### Update Frequencies
- **Collision Triggers**: No performance impact (event-driven)
- **Proximity Triggers**: 0.1s update interval recommended
- **Timer Triggers**: Minimal impact (time-based)

### Memory Usage
- **Event Channels**: ~200 bytes per channel
- **Triggers**: ~1-2 KB per trigger (varies by type)
- **Spatial Grid**: ~50 bytes per occupied cell

## 🛠️ Debugging Tools

### Trigger Debugging
- Enable **Debug Mode** on triggers for detailed logging
- Use **Context Menu > Test Trigger** to manually fire triggers
- **Gizmos** show trigger ranges and states

### Event Channel Debugging
- **Inspector Statistics** show raise count and subscriber count
- **Context Menu > Raise Event** for manual testing
- **Runtime Controls** in custom editors

### Performance Debugging
- **Performance Monitor** component tracks frame times
- **Trigger Manager** shows spatial grid and statistics
- **Profiler Integration** for detailed performance analysis

## 🏗️ Architecture Details

### Interface Hierarchy
```
ITrigger
├── BaseTrigger (abstract)
│   ├── CollisionTrigger
│   ├── ProximityTrigger
│   └── TimerTrigger

ITriggerAction
├── BaseTriggerAction (abstract)
│   ├── TransformActions
│   ├── GameObjectActions
│   └── EventChannelActions

ITriggerCondition
├── BaseTriggerCondition (abstract)
│   └── CommonConditions

IEventChannel
├── IGameEvent
│   └── GameEvent
└── IEventChannel<T>
    └── TypedEventChannel<T>
```

### Assembly Dependencies
```
GameFramework.Events
├── Depends on: GameFramework.Core
├── References: Unity.Mathematics
└── Editor assembly: GameFramework.Events.Editor
```

## 🔌 Extension Points

### Adding New Trigger Types
1. Inherit from `BaseTrigger`
2. Override `Initialize()` and update methods
3. Add `[AddComponentMenu]` attribute
4. Register with `TriggerManager` if needed

### Adding New Action Types
1. Inherit from `BaseTriggerAction`
2. Override `PerformAction(GameObject context)`
3. Add serialized fields for configuration
4. Add `[AddComponentMenu]` attribute

### Adding New Event Types
1. Inherit from `TypedEventChannel<T>`
2. Add `[CreateAssetMenu]` attribute
3. Create corresponding listener type
4. Add editor if needed

## 📋 Best Practices

### Organization
- Group related triggers under parent GameObjects
- Use clear, descriptive names for event channels
- Create folder structure for event assets
- Use prefabs for common trigger setups

### Performance
- Use TriggerManager for scenes with many triggers
- Set appropriate update intervals for proximity triggers
- Use conditions instead of complex trigger logic
- Pool frequently created/destroyed objects

### Debugging
- Enable debug mode during development
- Use descriptive names and descriptions
- Test triggers in isolation before integration
- Monitor performance with built-in tools

### Maintainability
- Document custom trigger and action purposes
- Use consistent naming conventions
- Keep trigger logic simple and focused
- Prefer composition over inheritance

## 🤝 Integration

### Visual Scripting
The system is designed to work seamlessly with Unity's Visual Scripting:
- Event channels can be accessed from Visual Scripting graphs
- Trigger events can flow into Visual Scripting nodes
- Custom nodes can be created for specific game logic

### Addressables
Event channels work with Addressables for dynamic loading:
- Event channels can be loaded as addressable assets
- Remote events can be raised through loaded channels
- Memory management is automatic

### Multiplayer
The system can be extended for multiplayer scenarios:
- Event channels can be synchronized across clients
- Triggers can be authority-checked
- Network events can be integrated

## 📄 License

This trigger and event system is part of the GameFramework project and follows the same licensing terms.

## 🙋 Support

For questions, issues, or feature requests:
1. Check the inline documentation and tooltips
2. Review the example scenes and prefabs
3. Use the built-in debugging tools
4. Consult the API documentation

The system is designed to be self-documenting through inspector help boxes, tooltips, and debug output.