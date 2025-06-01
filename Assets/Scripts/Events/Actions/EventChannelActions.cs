using UnityEngine;
using GameFramework.Events.Channels;
using System.Collections.Generic;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that raises multiple GameEvent channels.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise Game Event Action")]
    public class RaiseGameEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private List<GameEvent> gameEvents = new List<GameEvent>();
        [SerializeField] private bool createEventIfNull = false;
        [SerializeField] private bool raiseSequentially = false;
        [SerializeField] private float sequentialDelay = 0.1f;
        
        protected override void PerformAction(GameObject context)
        {
            if (gameEvents.Count == 0)
            {
                LogWarning("No GameEvents assigned to raise");
                return;
            }
            
            if (raiseSequentially && sequentialDelay > 0f)
            {
                StartCoroutine(RaiseEventsSequentially());
            }
            else
            {
                RaiseEventsImmediate();
            }
        }
        
        private void RaiseEventsImmediate()
        {
            int raisedCount = 0;
            foreach (var gameEvent in gameEvents)
            {
                if (gameEvent == null)
                {
                    if (createEventIfNull)
                    {
                        LogWarning("GameEvent is null and createEventIfNull is not implemented in editor context");
                    }
                    continue;
                }
                
                LogDebug($"Raising GameEvent: {gameEvent.ChannelName}");
                gameEvent.RaiseEvent();
                raisedCount++;
            }
            
            LogDebug($"Raised {raisedCount} GameEvents simultaneously");
        }
        
        private System.Collections.IEnumerator RaiseEventsSequentially()
        {
            int raisedCount = 0;
            
            for (int i = 0; i < gameEvents.Count; i++)
            {
                var gameEvent = gameEvents[i];
                
                if (gameEvent == null)
                {
                    if (createEventIfNull)
                    {
                        LogWarning("GameEvent is null and createEventIfNull is not implemented in editor context");
                    }
                    continue;
                }
                
                LogDebug($"Raising GameEvent {i + 1}/{gameEvents.Count}: {gameEvent.ChannelName}");
                gameEvent.RaiseEvent();
                raisedCount++;
                
                if (i < gameEvents.Count - 1 && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
            
            LogDebug($"Raised {raisedCount} GameEvents sequentially");
        }
        
        public void SetGameEvents(List<GameEvent> newEvents)
        {
            gameEvents = newEvents ?? new List<GameEvent>();
        }
        
        public void AddGameEvent(GameEvent newEvent)
        {
            if (newEvent != null && !gameEvents.Contains(newEvent))
            {
                gameEvents.Add(newEvent);
            }
        }
        
        public void RemoveGameEvent(GameEvent eventToRemove)
        {
            gameEvents.Remove(eventToRemove);
        }
        
        public List<GameEvent> GetGameEvents()
        {
            return new List<GameEvent>(gameEvents);
        }
        
        // Legacy API support
        public void SetGameEvent(GameEvent newEvent)
        {
            gameEvents.Clear();
            if (newEvent != null)
            {
                gameEvents.Add(newEvent);
            }
        }
        
        public GameEvent GetGameEvent()
        {
            return gameEvents.Count > 0 ? gameEvents[0] : null;
        }
    }
    
    /// <summary>
    /// Action that raises multiple int event channels.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise Int Event Action")]
    public class RaiseIntEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private List<IntEventChannel> intEvents = new List<IntEventChannel>();
        [SerializeField] private int value = 0;
        [SerializeField] private bool useRandomValue = false;
        [SerializeField] private int minValue = 0;
        [SerializeField] private int maxValue = 100;
        [SerializeField] private bool raiseSequentially = false;
        [SerializeField] private float sequentialDelay = 0.1f;
        
        protected override void PerformAction(GameObject context)
        {
            if (intEvents.Count == 0)
            {
                LogWarning("No IntEventChannels assigned to raise");
                return;
            }
            
            int eventValue = useRandomValue ? Random.Range(minValue, maxValue + 1) : value;
            
            if (raiseSequentially && sequentialDelay > 0f)
            {
                StartCoroutine(RaiseEventsSequentially(eventValue));
            }
            else
            {
                RaiseEventsImmediate(eventValue);
            }
        }
        
        private void RaiseEventsImmediate(int eventValue)
        {
            int raisedCount = 0;
            foreach (var intEvent in intEvents)
            {
                if (intEvent == null) continue;
                
                LogDebug($"Raising IntEvent: {intEvent.ChannelName} with value: {eventValue}");
                intEvent.RaiseEvent(eventValue);
                raisedCount++;
            }
            
            LogDebug($"Raised {raisedCount} IntEvents with value {eventValue} simultaneously");
        }
        
        private System.Collections.IEnumerator RaiseEventsSequentially(int eventValue)
        {
            int raisedCount = 0;
            
            for (int i = 0; i < intEvents.Count; i++)
            {
                var intEvent = intEvents[i];
                if (intEvent == null) continue;
                
                LogDebug($"Raising IntEvent {i + 1}/{intEvents.Count}: {intEvent.ChannelName} with value: {eventValue}");
                intEvent.RaiseEvent(eventValue);
                raisedCount++;
                
                if (i < intEvents.Count - 1 && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
            
            LogDebug($"Raised {raisedCount} IntEvents with value {eventValue} sequentially");
        }
        
        public void SetIntEvents(List<IntEventChannel> newEvents)
        {
            intEvents = newEvents ?? new List<IntEventChannel>();
        }
        
        public void AddIntEvent(IntEventChannel newEvent)
        {
            if (newEvent != null && !intEvents.Contains(newEvent))
            {
                intEvents.Add(newEvent);
            }
        }
        
        public void RemoveIntEvent(IntEventChannel eventToRemove)
        {
            intEvents.Remove(eventToRemove);
        }
        
        public List<IntEventChannel> GetIntEvents()
        {
            return new List<IntEventChannel>(intEvents);
        }
        
        // Legacy API support
        public void SetIntEvent(IntEventChannel newEvent)
        {
            intEvents.Clear();
            if (newEvent != null)
            {
                intEvents.Add(newEvent);
            }
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
    /// Action that raises multiple float event channels.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise Float Event Action")]
    public class RaiseFloatEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private List<FloatEventChannel> floatEvents = new List<FloatEventChannel>();
        [SerializeField] private float value = 0f;
        [SerializeField] private bool useRandomValue = false;
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;
        [SerializeField] private bool raiseSequentially = false;
        [SerializeField] private float sequentialDelay = 0.1f;
        
        protected override void PerformAction(GameObject context)
        {
            if (floatEvents.Count == 0)
            {
                LogWarning("No FloatEventChannels assigned to raise");
                return;
            }
            
            float eventValue = useRandomValue ? Random.Range(minValue, maxValue) : value;
            
            if (raiseSequentially && sequentialDelay > 0f)
            {
                StartCoroutine(RaiseEventsSequentially(eventValue));
            }
            else
            {
                RaiseEventsImmediate(eventValue);
            }
        }
        
        private void RaiseEventsImmediate(float eventValue)
        {
            int raisedCount = 0;
            foreach (var floatEvent in floatEvents)
            {
                if (floatEvent == null) continue;
                
                LogDebug($"Raising FloatEvent: {floatEvent.ChannelName} with value: {eventValue}");
                floatEvent.RaiseEvent(eventValue);
                raisedCount++;
            }
            
            LogDebug($"Raised {raisedCount} FloatEvents with value {eventValue} simultaneously");
        }
        
        private System.Collections.IEnumerator RaiseEventsSequentially(float eventValue)
        {
            int raisedCount = 0;
            
            for (int i = 0; i < floatEvents.Count; i++)
            {
                var floatEvent = floatEvents[i];
                if (floatEvent == null) continue;
                
                LogDebug($"Raising FloatEvent {i + 1}/{floatEvents.Count}: {floatEvent.ChannelName} with value: {eventValue}");
                floatEvent.RaiseEvent(eventValue);
                raisedCount++;
                
                if (i < floatEvents.Count - 1 && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
            
            LogDebug($"Raised {raisedCount} FloatEvents with value {eventValue} sequentially");
        }
        
        public void SetFloatEvents(List<FloatEventChannel> newEvents)
        {
            floatEvents = newEvents ?? new List<FloatEventChannel>();
        }
        
        public void AddFloatEvent(FloatEventChannel newEvent)
        {
            if (newEvent != null && !floatEvents.Contains(newEvent))
            {
                floatEvents.Add(newEvent);
            }
        }
        
        public void RemoveFloatEvent(FloatEventChannel eventToRemove)
        {
            floatEvents.Remove(eventToRemove);
        }
        
        public List<FloatEventChannel> GetFloatEvents()
        {
            return new List<FloatEventChannel>(floatEvents);
        }
        
        // Legacy API support
        public void SetFloatEvent(FloatEventChannel newEvent)
        {
            floatEvents.Clear();
            if (newEvent != null)
            {
                floatEvents.Add(newEvent);
            }
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
    /// Action that raises multiple string event channels.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise String Event Action")]
    public class RaiseStringEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private List<StringEventChannel> stringEvents = new List<StringEventChannel>();
        [SerializeField] private string value = "";
        [SerializeField] private bool useRandomString = false;
        [SerializeField] private string[] possibleValues = new string[0];
        [SerializeField] private bool raiseSequentially = false;
        [SerializeField] private float sequentialDelay = 0.1f;
        
        protected override void PerformAction(GameObject context)
        {
            if (stringEvents.Count == 0)
            {
                LogWarning("No StringEventChannels assigned to raise");
                return;
            }
            
            string eventValue = GetEventValue();
            
            if (raiseSequentially && sequentialDelay > 0f)
            {
                StartCoroutine(RaiseEventsSequentially(eventValue));
            }
            else
            {
                RaiseEventsImmediate(eventValue);
            }
        }
        
        private void RaiseEventsImmediate(string eventValue)
        {
            int raisedCount = 0;
            foreach (var stringEvent in stringEvents)
            {
                if (stringEvent == null) continue;
                
                LogDebug($"Raising StringEvent: {stringEvent.ChannelName} with value: {eventValue}");
                stringEvent.RaiseEvent(eventValue);
                raisedCount++;
            }
            
            LogDebug($"Raised {raisedCount} StringEvents with value '{eventValue}' simultaneously");
        }
        
        private System.Collections.IEnumerator RaiseEventsSequentially(string eventValue)
        {
            int raisedCount = 0;
            
            for (int i = 0; i < stringEvents.Count; i++)
            {
                var stringEvent = stringEvents[i];
                if (stringEvent == null) continue;
                
                LogDebug($"Raising StringEvent {i + 1}/{stringEvents.Count}: {stringEvent.ChannelName} with value: {eventValue}");
                stringEvent.RaiseEvent(eventValue);
                raisedCount++;
                
                if (i < stringEvents.Count - 1 && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
            
            LogDebug($"Raised {raisedCount} StringEvents with value '{eventValue}' sequentially");
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
        
        public void SetStringEvents(List<StringEventChannel> newEvents)
        {
            stringEvents = newEvents ?? new List<StringEventChannel>();
        }
        
        public void AddStringEvent(StringEventChannel newEvent)
        {
            if (newEvent != null && !stringEvents.Contains(newEvent))
            {
                stringEvents.Add(newEvent);
            }
        }
        
        public void RemoveStringEvent(StringEventChannel eventToRemove)
        {
            stringEvents.Remove(eventToRemove);
        }
        
        public List<StringEventChannel> GetStringEvents()
        {
            return new List<StringEventChannel>(stringEvents);
        }
        
        // Legacy API support
        public void SetStringEvent(StringEventChannel newEvent)
        {
            stringEvents.Clear();
            if (newEvent != null)
            {
                stringEvents.Add(newEvent);
            }
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
    /// Action that raises multiple Vector3 event channels.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise Vector3 Event Action")]
    public class RaiseVector3EventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private List<Vector3EventChannel> vector3Events = new List<Vector3EventChannel>();
        [SerializeField] private Vector3 value = Vector3.zero;
        [SerializeField] private bool useTransformPosition = false;
        [SerializeField] private Transform sourceTransform;
        [SerializeField] private bool useContextPosition = false;
        [SerializeField] private bool raiseSequentially = false;
        [SerializeField] private float sequentialDelay = 0.1f;
        
        protected override void PerformAction(GameObject context)
        {
            if (vector3Events.Count == 0)
            {
                LogWarning("No Vector3EventChannels assigned to raise");
                return;
            }
            
            Vector3 eventValue = GetEventValue(context);
            
            if (raiseSequentially && sequentialDelay > 0f)
            {
                StartCoroutine(RaiseEventsSequentially(eventValue));
            }
            else
            {
                RaiseEventsImmediate(eventValue);
            }
        }
        
        private void RaiseEventsImmediate(Vector3 eventValue)
        {
            int raisedCount = 0;
            foreach (var vector3Event in vector3Events)
            {
                if (vector3Event == null) continue;
                
                LogDebug($"Raising Vector3Event: {vector3Event.ChannelName} with value: {eventValue}");
                vector3Event.RaiseEvent(eventValue);
                raisedCount++;
            }
            
            LogDebug($"Raised {raisedCount} Vector3Events with value {eventValue} simultaneously");
        }
        
        private System.Collections.IEnumerator RaiseEventsSequentially(Vector3 eventValue)
        {
            int raisedCount = 0;
            
            for (int i = 0; i < vector3Events.Count; i++)
            {
                var vector3Event = vector3Events[i];
                if (vector3Event == null) continue;
                
                LogDebug($"Raising Vector3Event {i + 1}/{vector3Events.Count}: {vector3Event.ChannelName} with value: {eventValue}");
                vector3Event.RaiseEvent(eventValue);
                raisedCount++;
                
                if (i < vector3Events.Count - 1 && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
            
            LogDebug($"Raised {raisedCount} Vector3Events with value {eventValue} sequentially");
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
        
        public void SetVector3Events(List<Vector3EventChannel> newEvents)
        {
            vector3Events = newEvents ?? new List<Vector3EventChannel>();
        }
        
        public void AddVector3Event(Vector3EventChannel newEvent)
        {
            if (newEvent != null && !vector3Events.Contains(newEvent))
            {
                vector3Events.Add(newEvent);
            }
        }
        
        public void RemoveVector3Event(Vector3EventChannel eventToRemove)
        {
            vector3Events.Remove(eventToRemove);
        }
        
        public List<Vector3EventChannel> GetVector3Events()
        {
            return new List<Vector3EventChannel>(vector3Events);
        }
        
        // Legacy API support
        public void SetVector3Event(Vector3EventChannel newEvent)
        {
            vector3Events.Clear();
            if (newEvent != null)
            {
                vector3Events.Add(newEvent);
            }
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
    /// Action that raises multiple GameObject event channels.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Raise GameObject Event Action")]
    public class RaiseGameObjectEventAction : BaseTriggerAction
    {
        [Header("Event Settings")]
        [SerializeField] private List<GameObjectEventChannel> gameObjectEvents = new List<GameObjectEventChannel>();
        [SerializeField] private GameObject value;
        [SerializeField] private bool useContext = false;
        [SerializeField] private bool useSelf = false;
        [SerializeField] private bool raiseSequentially = false;
        [SerializeField] private float sequentialDelay = 0.1f;
        
        protected override void PerformAction(GameObject context)
        {
            if (gameObjectEvents.Count == 0)
            {
                LogWarning("No GameObjectEventChannels assigned to raise");
                return;
            }
            
            GameObject eventValue = GetEventValue(context);
            
            if (raiseSequentially && sequentialDelay > 0f)
            {
                StartCoroutine(RaiseEventsSequentially(eventValue));
            }
            else
            {
                RaiseEventsImmediate(eventValue);
            }
        }
        
        private void RaiseEventsImmediate(GameObject eventValue)
        {
            int raisedCount = 0;
            foreach (var gameObjectEvent in gameObjectEvents)
            {
                if (gameObjectEvent == null) continue;
                
                LogDebug($"Raising GameObjectEvent: {gameObjectEvent.ChannelName} with value: {(eventValue ? eventValue.name : "null")}");
                gameObjectEvent.RaiseEvent(eventValue);
                raisedCount++;
            }
            
            LogDebug($"Raised {raisedCount} GameObjectEvents with value '{(eventValue ? eventValue.name : "null")}' simultaneously");
        }
        
        private System.Collections.IEnumerator RaiseEventsSequentially(GameObject eventValue)
        {
            int raisedCount = 0;
            
            for (int i = 0; i < gameObjectEvents.Count; i++)
            {
                var gameObjectEvent = gameObjectEvents[i];
                if (gameObjectEvent == null) continue;
                
                LogDebug($"Raising GameObjectEvent {i + 1}/{gameObjectEvents.Count}: {gameObjectEvent.ChannelName} with value: {(eventValue ? eventValue.name : "null")}");
                gameObjectEvent.RaiseEvent(eventValue);
                raisedCount++;
                
                if (i < gameObjectEvents.Count - 1 && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
            
            LogDebug($"Raised {raisedCount} GameObjectEvents with value '{(eventValue ? eventValue.name : "null")}' sequentially");
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
        
        public void SetGameObjectEvents(List<GameObjectEventChannel> newEvents)
        {
            gameObjectEvents = newEvents ?? new List<GameObjectEventChannel>();
        }
        
        public void AddGameObjectEvent(GameObjectEventChannel newEvent)
        {
            if (newEvent != null && !gameObjectEvents.Contains(newEvent))
            {
                gameObjectEvents.Add(newEvent);
            }
        }
        
        public void RemoveGameObjectEvent(GameObjectEventChannel eventToRemove)
        {
            gameObjectEvents.Remove(eventToRemove);
        }
        
        public List<GameObjectEventChannel> GetGameObjectEvents()
        {
            return new List<GameObjectEventChannel>(gameObjectEvents);
        }
        
        // Legacy API support
        public void SetGameObjectEvent(GameObjectEventChannel newEvent)
        {
            gameObjectEvents.Clear();
            if (newEvent != null)
            {
                gameObjectEvents.Add(newEvent);
            }
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