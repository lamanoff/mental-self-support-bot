namespace Bot;

public class UserContext
{
    public ThoughtsMap ThoughtsMap { get; set; } = new();
    public State State { get; set; } = State.None;
}