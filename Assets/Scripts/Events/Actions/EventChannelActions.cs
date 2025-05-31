using UnityEngine;
using GameFramework.Events.Channels;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that raises a GameEvent channel.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise Game Event Action")]
    public class RaiseGameEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private bool createEventIfNull = false;
        
        protected override void PerformAction(GameObject context)
        {
            if (gameEvent == null)
            {
                if (createEventIfNull)
                {
                    LogWarning("GameEvent is null and createEventIfNull is not implemented in editor context");
                }
                else
                {
                    LogWarning("Cannot raise event - GameEvent is null");
                }
                return;
            }
            
            LogDebug($"Raising GameEvent: {gameEvent.ChannelName}");
            gameEvent.RaiseEvent();
        }
        
        public void SetGameEvent(GameEvent newEvent)
        {
            gameEvent = newEvent;
        }
        
        public GameEvent GetGameEvent()
        {
            return gameEvent;
        }
    }
    
    /// <summary>
    /// Action that raises an int event channel.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise Int Event Action")]
    public class RaiseIntEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private IntEventChannel intEvent;
        [SerializeField] private int value = 0;
        [SerializeField] private bool useRandomValue = false;
        [SerializeField] private int minValue = 0;
        [SerializeField] private int maxValue = 100;
        
        protected override void PerformAction(GameObject context)
        {
            if (intEvent == null)
            {
                LogWarning("Cannot raise event - IntEventChannel is null");
                return;
            }
            
            int eventValue = useRandomValue ? Random.Range(minValue, maxValue + 1) : value;
            
            LogDebug($"Raising IntEvent: {intEvent.ChannelName} with value: {eventValue}");
            intEvent.RaiseEvent(eventValue);
        }
        
        public void SetIntEvent(IntEventChannel newEvent)
        {
            intEvent = newEvent;
        }
        
        public void SetValue(int newValue)
        {
            value = newValue;
            useRandomValue = false;
        }
        
        public void SetRandomRange(int min, int max)
        {
            minValue = min;
            maxValue = max;
            useRandomValue = true;
        }
    }
    
    /// <summary>
    /// Action that raises a float event channel.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise Float Event Action")]
    public class RaiseFloatEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private FloatEventChannel floatEvent;
        [SerializeField] private float value = 0f;
        [SerializeField] private bool useRandomValue = false;
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;
        
        protected override void PerformAction(GameObject context)
        {
            if (floatEvent == null)
            {
                LogWarning("Cannot raise event - FloatEventChannel is null");
                return;
            }
            
            float eventValue = useRandomValue ? Random.Range(minValue, maxValue) : value;
            
            LogDebug($"Raising FloatEvent: {floatEvent.ChannelName} with value: {eventValue}");
            floatEvent.RaiseEvent(eventValue);
        }
        
        public void SetFloatEvent(FloatEventChannel newEvent)
        {
            floatEvent = newEvent;
        }
        
        public void SetValue(float newValue)
        {
            value = newValue;
            useRandomValue = false;
        }
        
        public void SetRandomRange(float min, float max)
        {
            minValue = min;
            maxValue = max;
            useRandomValue = true;
        }
    }
    
    /// <summary>
    /// Action that raises a string event channel.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise String Event Action")]
    public class RaiseStringEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private StringEventChannel stringEvent;
        [SerializeField] private string value = "";
        [SerializeField] private bool useRandomString = false;
        [SerializeField] private string[] possibleValues = new string[0];
        
        protected override void PerformAction(GameObject context)
        {
            if (stringEvent == null)
            {
                LogWarning("Cannot raise event - StringEventChannel is null");
                return;
            }
            
            string eventValue = GetEventValue();
            
            LogDebug($"Raising StringEvent: {stringEvent.ChannelName} with value: {eventValue}");
            stringEvent.RaiseEvent(eventValue);
        }
        
        private string GetEventValue()
        {
            if (useRandomString && possibleValues.Length > 0)
            {
                int randomIndex = Random.Range(0, possibleValues.Length);
                return possibleValues[randomIndex];
            }
            
            return value;
        }
        
        public void SetStringEvent(StringEventChannel newEvent)
        {
            stringEvent = newEvent;
        }
        
        public void SetValue(string newValue)
        {
            value = newValue;
            useRandomString = false;
        }
        
        public void SetPossibleValues(string[] values)
        {
            possibleValues = values;
            useRandomString = true;
        }
    }
    
    /// <summary>
    /// Action that raises a Vector3 event channel.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise Vector3 Event Action")]
    public class RaiseVector3EventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private Vector3EventChannel vector3Event;
        [SerializeField] private Vector3 value = Vector3.zero;
        [SerializeField] private bool useTransformPosition = false;
        [SerializeField] private Transform sourceTransform;
        [SerializeField] private bool useContextPosition = false;
        
        protected override void PerformAction(GameObject context)
        {
            if (vector3Event == null)
            {
                LogWarning("Cannot raise event - Vector3EventChannel is null");
                return;
            }
            
            Vector3 eventValue = GetEventValue(context);
            
            LogDebug($"Raising Vector3Event: {vector3Event.ChannelName} with value: {eventValue}");
            vector3Event.RaiseEvent(eventValue);
        }
        
        private Vector3 GetEventValue(GameObject context)
        {
            if (useContextPosition && context != null)
            {
                return context.transform.position;
            }
            
            if (useTransformPosition && sourceTransform != null)
            {
                return sourceTransform.position;
            }
            
            return value;
        }
        
        public void SetVector3Event(Vector3EventChannel newEvent)
        {
            vector3Event = newEvent;
        }
        
        public void SetValue(Vector3 newValue)
        {
            value = newValue;
            useTransformPosition = false;
            useContextPosition = false;
        }
        
        public void SetSourceTransform(Transform transform)
        {
            sourceTransform = transform;
            useTransformPosition = true;
            useContextPosition = false;
        }
    }
    
    /// <summary>
    /// Action that raises a GameObject event channel.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise GameObject Event Action")]
    public class RaiseGameObjectEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private GameObjectEventChannel gameObjectEvent;
        [SerializeField] private GameObject value;
        [SerializeField] private bool useContext = false;
        [SerializeField] private bool useSelf = false;
        
        protected override void PerformAction(GameObject context)
        {
            if (gameObjectEvent == null)
            {
                LogWarning("Cannot raise event - GameObjectEventChannel is null");
                return;
            }
            
            GameObject eventValue = GetEventValue(context);
            
            LogDebug($"Raising GameObjectEvent: {gameObjectEvent.ChannelName} with value: {(eventValue ? eventValue.name : "null")}");
            gameObjectEvent.RaiseEvent(eventValue);
        }
        
        private GameObject GetEventValue(GameObject context)
        {
            if (useContext && context != null)
            {
                return context;
            }
            
            if (useSelf)
            {
                return gameObject;
            }
            
            return value;
        }
        
        public void SetGameObjectEvent(GameObjectEventChannel newEvent)
        {
            gameObjectEvent = newEvent;
        }
        
        public void SetValue(GameObject newValue)
        {
            value = newValue;
            useContext = false;
            useSelf = false;
        }
        
        public void SetUseContext(bool use)
        {
            useContext = use;
            useSelf = false;
        }
        
        public void SetUseSelf(bool use)
        {
            useSelf = use;
            useContext = false;
        }
    }
}