using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

interface IPublish : ICompile
{
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath PublishDirectory => ArtifactsDirectory / "publish";

    [Parameter("Runtime identifier for publishing")]
    string Runtime => TryGetValue(() => Runtime) ?? "win-x64";

    [Parameter("Self-contained deployment")]
    bool SelfContained => TryGetValue(() => SelfContained) ?? true;

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var consoleProject = Solution.GetProject("LablabBean.Console");
            if (consoleProject != null)
            {
                DotNetPublish(s => s
                    .SetProject(consoleProject)
                    .SetConfiguration(Configuration)
                    .SetOutput(PublishDirectory / "console")
                    .SetRuntime(Runtime)
                    .SetSelfContained(SelfContained)
                    .EnableNoRestore());
            }

            var windowsProject = Solution.GetProject("LablabBean.Windows");
            if (windowsProject != null)
            {
                DotNetPublish(s => s
                    .SetProject(windowsProject)
                    .SetConfiguration(Configuration)
                    .SetOutput(PublishDirectory / "windows")
                    .SetRuntime(Runtime)
                    .SetSelfContained(SelfContained)
                    .EnableNoRestore());
            }
        });
}
