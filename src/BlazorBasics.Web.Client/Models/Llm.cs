namespace BlazorBasics.Web.Client.Models;

public class Llm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Tagline { get; set; } = "";
    public int Votes { get; set; }
    public Dictionary<string, decimal> Scores { get; set; } = new();
}
