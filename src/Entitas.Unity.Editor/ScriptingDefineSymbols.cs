using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;

namespace Entitas.Unity.Editor
{
    /// Adds and removes scripting define symbols for every (non obsolete) build target group.
    class ScriptingDefineSymbols
    {
        public static BuildTargetGroup[] BuildTargetGroups => Enum.GetNames(typeof(BuildTargetGroup))
            .Where(name => !Attribute.IsDefined(typeof(BuildTargetGroup).GetField(name), typeof(ObsoleteAttribute)))
            .Select(name => (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), name))
            .Where(group => group != BuildTargetGroup.Unknown)
            .ToArray();

        public static bool HasDefine(BuildTargetGroup buildTargetGroup, string defineSymbol) =>
            GetDefines(buildTargetGroup).Contains(defineSymbol);

        public void AddForAll(string defineSymbol)
        {
            foreach (var buildTargetGroup in BuildTargetGroups)
                Add(defineSymbol, buildTargetGroup);
        }

        public void RemoveForAll(string defineSymbol)
        {
            foreach (var buildTargetGroup in BuildTargetGroups)
                Remove(defineSymbol, buildTargetGroup);
        }

        void Add(string defineSymbol, BuildTargetGroup buildTargetGroup)
        {
            var defines = Regex.Replace(GetDefines(buildTargetGroup), "\\b" + defineSymbol + "\\b", string.Empty);
            SetDefines(buildTargetGroup, defines + ";" + defineSymbol);
        }

        void Remove(string defineSymbol, BuildTargetGroup buildTargetGroup)
        {
            var defines = Regex.Replace(GetDefines(buildTargetGroup), "\\b" + defineSymbol + "\\b", string.Empty);
            SetDefines(buildTargetGroup, defines);
        }

        static string GetDefines(BuildTargetGroup buildTargetGroup) =>
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));

        static void SetDefines(BuildTargetGroup buildTargetGroup, string defines) =>
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), defines);
    }
}
