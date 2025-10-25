using Akka.Actor;
using Akka.Event;
using LablabBean.AI.Core.Events;

namespace LablabBean.AI.Actors.Bridges;

/// <summary>
/// Actor that bridges Akka.NET messages to ECS event bus
/// Receives PublishGameEvent messages and publishes them to the event bus
/// </summary>
public class EventBusAkkaAdapter : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Action<object>? _eventPublisher;

    public EventBusAkkaAdapter(Action<object>? eventPublisher = null)
    {
        _eventPublisher = eventPublisher;

        Receive<Messages.PublishGameEvent>(msg =>
        {
            try
            {
                if (_eventPublisher != null)
                {
                    _eventPublisher(msg.Event);
                    _log.Debug("Published event {0} to game event bus", msg.Event.GetType().Name);
                }
                else
                {
                    _log.Warning("No event publisher configured, event {0} not published", msg.Event.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error publishing event {0}", msg.Event.GetType().Name);
            }
        });

        Receive<AIThoughtEvent>(evt =>
        {
            _log.Debug("[THOUGHT] {0}: {1}", evt.EntityId, evt.Thought);
        });

        Receive<AIBehaviorChangedEvent>(evt =>
        {
            _log.Info("[BEHAVIOR] {0}: {1} -> {2} ({3})",
                evt.EntityId, evt.PreviousBehavior, evt.NewBehavior, evt.Reason);
        });

        Receive<NPCDialogueEvent>(evt =>
        {
            _log.Info("[DIALOGUE] {0} -> {1}: {2}",
                evt.EntityId, evt.TargetEntityId, evt.DialogueText);
        });

        Receive<ActorStoppedEvent>(evt =>
        {
            _log.Info("[STOPPED] {0} ({1}): {2}", evt.ActorPath, evt.EntityId, evt.Reason);
        });
    }

    public static Props Props(Action<object>? eventPublisher = null) =>
        Akka.Actor.Props.Create(() => new EventBusAkkaAdapter(eventPublisher));
}
