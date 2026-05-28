namespace BlazorBasics.Web.Client.Services;

public class GreetingService
{
    private static readonly string[] Greetings =
    [
        "Hello, world!",
        "Hi there, friend.",
        "Greetings, traveler.",
        "Hey! Good to see you.",
        "Howdy, partner.",
        "Salutations.",
        "Welcome back.",
        "Ahoy!",
        "What's up?",
        "Good day to you.",
    ];

    public string Get() => Greetings[Random.Shared.Next(Greetings.Length)];
}
