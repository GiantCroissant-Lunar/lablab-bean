using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

interface ICompile : INukeBuild
{
    [Parameter("Configuration to build")]
    Configuration Configuration => TryGetValue(() => Configuration) ?? Configuration.Debug;

    [Solution]
    Solution Solution => TryGetValue(() => Solution);

    Target Compile => _ => _
        .DependsOn<IRestore>()
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
}
