using Akka.Actor;

namespace LablabBean.AI.Core.Components;

/// <summary>
/// ECS component that holds a reference to an Akka.NET actor
/// </summary>
public struct AkkaActorRef
{
    public IActorRef ActorRef { get; set; }
    public string ActorPath { get; set; }

    public AkkaActorRef(IActorRef actorRef, string actorPath)
    {
        ActorRef = actorRef;
        ActorPath = actorPath;
    }
}
