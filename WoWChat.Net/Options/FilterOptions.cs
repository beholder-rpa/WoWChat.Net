namespace WoWChat.Net.Options
{
  public class FilterOptions
  {
    public bool Enabled { get; set; } = true;

    public string[] Patterns { get; set; } = Array.Empty<string>();
  }
}
