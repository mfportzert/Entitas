// Contexts.g.cs
public partial class Contexts : Entitas.IContexts
{
    public static Contexts sharedInstance
    {
        get
        {
            if (_sharedInstance == null)
            {
                _sharedInstance = new Contexts();
            }

            return _sharedInstance;
        }
        set { _sharedInstance = value; }
    }

    static Contexts _sharedInstance;

    public GameContext game { get; set; }

    public Entitas.IContext[] allContexts { get { return new Entitas.IContext [] { game }; } }

    public Contexts()
    {
        game = new GameContext();

        var postConstructors = System.Linq.Enumerable.Where(
            GetType().GetMethods(),
            method => System.Attribute.IsDefined(method, typeof(Entitas.CodeGeneration.Attributes.PostConstructorAttribute))
        );

        foreach (var postConstructor in postConstructors)
        {
            postConstructor.Invoke(this, null);
        }
    }

    public void Reset()
    {
        var contexts = allContexts;
        for (int i = 0; i < contexts.Length; i++)
        {
            contexts[i].Reset();
        }
    }
}


// GameCleanupSystems.g.cs
public sealed class GameCleanupSystems : Feature
{
    public GameCleanupSystems(Contexts contexts)
    {

    }
}


// GameComponentsLookup.g.cs
public static class GameComponentsLookup
{
    public const int TestEvent = 0;
    public const int TestEventListener = 1;

    public const int TotalComponents = 2;

    public static readonly string[] componentNames = 
    {
        "TestEvent",
        "TestEventListener"
    };

    public static readonly System.Type[] componentTypes = 
    {
        typeof(TestEventComponent),
        typeof(TestEventListenerComponent)
    };
}


// GameContext.g.cs
public sealed partial class GameContext : Entitas.Context<GameEntity>
{
    public GameContext()
        : base(
            GameComponentsLookup.TotalComponents,
            0,
            new Entitas.ContextInfo(
                "Game",
                GameComponentsLookup.componentNames,
                GameComponentsLookup.componentTypes
            ),
            (entity) =>

#if (ENTITAS_FAST_AND_UNSAFE)
                new Entitas.UnsafeAERC(),
#else
                new Entitas.SafeAERC(entity),
#endif
            () => new GameEntity()
        ) 
    {
    }
}


// GameEntity.g.cs
public sealed partial class GameEntity : Entitas.Entity
{
}


// GameEventSystems.g.cs
public sealed class GameEventSystems : Feature
{
    public GameEventSystems(Contexts contexts)
    {
        Add(new TestEventEventSystem(contexts)); // priority: 0
    }
}


// GameMatcher.g.cs
public sealed partial class GameMatcher 
{
    public static Entitas.IAllOfMatcher<GameEntity> AllOf(params int[] indices) 
    {
        return Entitas.Matcher<GameEntity>.AllOf(indices);
    }

    public static Entitas.IAllOfMatcher<GameEntity> AllOf(params Entitas.IMatcher<GameEntity>[] matchers)
    {
        return Entitas.Matcher<GameEntity>.AllOf(matchers);
    }

    public static Entitas.IAnyOfMatcher<GameEntity> AnyOf(params int[] indices)
    {
        return Entitas.Matcher<GameEntity>.AnyOf(indices);
    }

    public static Entitas.IAnyOfMatcher<GameEntity> AnyOf(params Entitas.IMatcher<GameEntity>[] matchers)
    {
        return Entitas.Matcher<GameEntity>.AnyOf(matchers);
    }
}


// GameTestEventComponent.g.cs
public partial class GameEntity
{
    static readonly TestEventComponent testEventComponent = new TestEventComponent();

    public bool isTestEvent
    {
        get { return HasComponent(GameComponentsLookup.TestEvent); }
        set 
        {
            if (value != isTestEvent)
            {
                var index = GameComponentsLookup.TestEvent;
                if (value)
                {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : testEventComponent;

                    AddComponent(index, component);
                }
                else
                {
                    RemoveComponent(index);
                }
            }
        }
    }
}

public sealed partial class GameMatcher
{
    static Entitas.IMatcher<GameEntity> _matcherTestEvent;

    public static Entitas.IMatcher<GameEntity> TestEvent
    {
        get
        {
            if (_matcherTestEvent == null)
            {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.TestEvent);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherTestEvent = matcher;
            }

            return _matcherTestEvent;
        }
    }
}


// GameTestEventListenerComponent.g.cs
public partial class GameEntity
{
    public TestEventListenerComponent testEventListener { get { return (TestEventListenerComponent)GetComponent(GameComponentsLookup.TestEventListener); } }
    public bool hasTestEventListener { get { return HasComponent(GameComponentsLookup.TestEventListener); } }

    public void AddTestEventListener(System.Collections.Generic.List<ITestEventListener> newValue)
    {
        var index = GameComponentsLookup.TestEventListener;
        var component = (TestEventListenerComponent)CreateComponent(index, typeof(TestEventListenerComponent));
        component.value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceTestEventListener(System.Collections.Generic.List<ITestEventListener> newValue)
    {
        var index = GameComponentsLookup.TestEventListener;
        var component = (TestEventListenerComponent)CreateComponent(index, typeof(TestEventListenerComponent));
        component.value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveTestEventListener()
    {
        RemoveComponent(GameComponentsLookup.TestEventListener);
    }
}

public sealed partial class GameMatcher
{
    static Entitas.IMatcher<GameEntity> _matcherTestEventListener;

    public static Entitas.IMatcher<GameEntity> TestEventListener
    {
        get
        {
            if (_matcherTestEventListener == null)
            {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.TestEventListener);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherTestEventListener = matcher;
            }

            return _matcherTestEventListener;
        }
    }
}


// GameTestEventListenerComponentEvent.g.cs
public partial class GameEntity
{
    public void AddTestEventListener(ITestEventListener value)
    {
        var listeners = hasTestEventListener
            ? testEventListener.value
            : new System.Collections.Generic.List<ITestEventListener>();
        listeners.Add(value);
        ReplaceTestEventListener(listeners);
    }

    public void RemoveTestEventListener(ITestEventListener value, bool removeComponentWhenEmpty = true)
    {
        var listeners = testEventListener.value;
        listeners.Remove(value);
        if (removeComponentWhenEmpty && listeners.Count == 0)
        {
            RemoveTestEventListener();
        }
        else
        {
            ReplaceTestEventListener(listeners);
        }
    }
}


// ITestEventListener.g.cs
public interface ITestEventListener
{
    void OnTestEvent(GameEntity entity);
}


// TestEventEventSystem.g.cs
public sealed class TestEventEventSystem : Entitas.ReactiveSystem<GameEntity>
{
    readonly System.Collections.Generic.List<ITestEventListener> _listenerBuffer;

    public TestEventEventSystem(Contexts contexts) : base(contexts.game)
    {
        _listenerBuffer = new System.Collections.Generic.List<ITestEventListener>();
    }

    protected override Entitas.ICollector<GameEntity> GetTrigger(Entitas.IContext<GameEntity> context)
    {
        return Entitas.CollectorContextExtension.CreateCollector(
            context, Entitas.TriggerOnEventMatcherExtension.Added(GameMatcher.TestEvent)
        );
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.isTestEvent && entity.hasTestEventListener;
    }

    protected override void Execute(System.Collections.Generic.List<GameEntity> entities)
    {
        foreach (var e in entities)
        {
            
            _listenerBuffer.Clear();
            _listenerBuffer.AddRange(e.testEventListener.value);
            foreach (var listener in _listenerBuffer)
            {
                listener.OnTestEvent(e);
            }
        }
    }
}


// TestEventListenerComponent.g.cs
[Entitas.CodeGeneration.Attributes.DontGenerate]
public sealed class TestEventListenerComponent : Entitas.IComponent
{
    public System.Collections.Generic.List<ITestEventListener> value;
}
