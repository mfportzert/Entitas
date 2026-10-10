namespace Entitas.CodeGeneration.VisualDebugging.Feature;

public static class FeatureTemplates
{
    public const string FeatureTemplate =
        @"#if (!ENTITAS_DISABLE_VISUAL_DEBUGGING && UNITY_EDITOR)

public class Feature : Entitas.Unity.DebugSystems
{
    public Feature(string name) : base(name) { }

    public Feature() : base(true) => Initialize(GetType().FullName);
}

#elif (!ENTITAS_DISABLE_DEEP_PROFILING && ENABLE_PROFILER)

public class Feature : Entitas.Systems
{
    // The names are cached when a system is added: Type.FullName can allocate on each call
    // and the profiler samples run for every system, every frame.
    readonly System.Collections.Generic.List<string> _initializeSystemNames = new System.Collections.Generic.List<string>();
    readonly System.Collections.Generic.List<string> _executeSystemNames = new System.Collections.Generic.List<string>();
    readonly System.Collections.Generic.List<string> _cleanupSystemNames = new System.Collections.Generic.List<string>();
    readonly System.Collections.Generic.List<string> _tearDownSystemNames = new System.Collections.Generic.List<string>();

    public Feature(string name) : this() { }

    public Feature() { }

    public override Entitas.Systems Add(Entitas.ISystem system)
    {
        var systemName = system.GetType().FullName;

        if (system is Entitas.IInitializeSystem)
            _initializeSystemNames.Add(systemName);

        if (system is Entitas.IExecuteSystem)
            _executeSystemNames.Add(systemName);

        if (system is Entitas.ICleanupSystem)
            _cleanupSystemNames.Add(systemName);

        if (system is Entitas.ITearDownSystem)
            _tearDownSystemNames.Add(systemName);

        return base.Add(system);
    }

    public override void Initialize()
    {
        for (var i = 0; i < _initializeSystems.Count; i++)
        {
            UnityEngine.Profiling.Profiler.BeginSample(_initializeSystemNames[i]);
            _initializeSystems[i].Initialize();
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public override void Execute()
    {
        for (var i = 0; i < _executeSystems.Count; i++)
        {
            UnityEngine.Profiling.Profiler.BeginSample(_executeSystemNames[i]);
            _executeSystems[i].Execute();
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public override void Cleanup()
    {
        for (var i = 0; i < _cleanupSystems.Count; i++)
        {
            UnityEngine.Profiling.Profiler.BeginSample(_cleanupSystemNames[i]);
            _cleanupSystems[i].Cleanup();
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public override void TearDown()
    {
        for (var i = 0; i < _tearDownSystems.Count; i++)
        {
            UnityEngine.Profiling.Profiler.BeginSample(_tearDownSystemNames[i]);
            _tearDownSystems[i].TearDown();
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}

#else

public class Feature : Entitas.Systems
{
    public Feature(string name) { }

    public Feature() { }
}

#endif
";
}
