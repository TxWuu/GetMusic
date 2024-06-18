using Newtonsoft.Json;

public class Song
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("artist")]
    public string Artist { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("pic")]
    public string Pic { get; set; }

    [JsonProperty("lrc")]
    public string Lrc { get; set; }
}