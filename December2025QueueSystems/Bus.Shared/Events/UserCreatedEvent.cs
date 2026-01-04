namespace Bus.Shared.Events
{
    public record UserCreatedEvent(int UserId, string UserName, string Email) : BaseEvent;
}
