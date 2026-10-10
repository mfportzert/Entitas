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
    public Feature(string name) : this() { }

        public Feature() { }

        public override void Initialize()
        {
            for (var i = 0; i < _initializeSystems.Count; i++)
            {
                var system = _initializeSystems[i];
                UnityEngine.Profiling.Profiler.BeginSample(system.GetType().FullName);
                system.Initialize();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public override void Execute()
        {
            for (var i = 0; i < _executeSystems.Count; i++)
            {
                var system = _executeSystems[i];
                UnityEngine.Profiling.Profiler.BeginSample(system.GetType().FullName);
                system.Execute();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public override void Cleanup()
        {
            for (var i = 0; i < _cleanupSystems.Count; i++)
            {
                var system = _cleanupSystems[i];
                UnityEngine.Profiling.Profiler.BeginSample(system.GetType().FullName);
                system.Cleanup();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public override void TearDown()
        {
            for (var i = 0; i < _tearDownSystems.Count; i++)
            {
                var system = _tearDownSystems[i];
                UnityEngine.Profiling.Profiler.BeginSample(system.GetType().FullName);
                system.TearDown();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
    }
#else
    public class Feature : Systems
    {
        public Feature(string name) { }

        public Feature() { }
    }
#endif
";
}
