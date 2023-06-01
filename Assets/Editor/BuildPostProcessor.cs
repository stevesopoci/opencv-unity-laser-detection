using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class BuildPostProcessor
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            PBXProject project = new PBXProject();
            string targetName = project.GetUnityMainTargetGuid();
            string projectTarget = project.TargetGuidByName(targetName);

            AddFrameworks(project, projectTarget);
        }
    }

    static void AddFrameworks(PBXProject project, string target)
    {
        // Add `-ObjC` to "Other Linker Flags".
        project.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
    }
}
