using System.Reflection;
using UnityEditor;

namespace WaynGroup.Mgm.Ability.Editor
{
    internal class ScriptTemplates
    {
        [MenuItem("Assets/Create/DOTS/Unmanaged System")]
        public static void CreateUnmanagedSystem()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/ScriptTemplates/UnmanagedSystem.txt",
                "UnSystem.cs");
        }

        [MenuItem("Assets/Create/DOTS/Authoring Component")]
        public static void CreateAuthoringComponent()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/ScriptTemplates/AuthoringComponent.txt",
                "Authoring.cs");
        }

        [MenuItem("Assets/Create/DOTS/IComponentData")]
        public static void CreateIComponentData()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/ScriptTemplates/IComponentData.txt",
                "IData.cs");
        }
        [MenuItem("Assets/Create/DOTS/IBufferElementData")]
        public static void CreateIBufferElementData()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/ScriptTemplates/IBufferElementData.txt",
                "IBufferData.cs");
        }
        [MenuItem("Assets/Create/DOTS/Hybrid Component")]
        public static void CreateHybridComponent()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/ScriptTemplates/HybridComponent.txt",
                "HybridComponent.cs");
        }

    }
}