namespace LablabBean.DependencyInjection.Diagnostics;

internal enum ResolveOutcome
{
    LocalHit = 0,
    ParentHit = 1,
    NotFound = 2
}
