namespace ExternalApiCache.Models;

public sealed class Character
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public CharacterOrigin Origin { get; set; } = new CharacterOrigin();
    public CharacterLocation Location { get; set; } = new CharacterLocation();
    public string Image { get; set; } = string.Empty;
    public List<string> Episode { get; set; } = new List<string>();
    public string Url { get; set; } = string.Empty;
    public string Created { get; set; } = string.Empty;
}

public sealed class CharacterOrigin
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class CharacterLocation
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}