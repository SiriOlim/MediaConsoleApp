// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using System.Net.Http.Headers;

async Task<List<Media>> LoadMedia()
{
    HttpClient client = new HttpClient();
    client.BaseAddress = new Uri("https://mreyes-musiclibrary-api.azurewebsites.net/");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    HttpResponseMessage response = await client.GetAsync("api/Media");
    var responseData = await response.Content.ReadAsStringAsync();
    var apiResource = JsonConvert.DeserializeObject<ApiResponseResource>(responseData);
    return apiResource?.MediaResources?.ToList() ?? new();
}

bool IsOver60Minutes(Media media)
{
    var isOver60 = false;
    isOver60 = media?.Songs?.Sum(x => x.Duration) > 60;
    return isOver60;
}

// LOAD FROM MEDIA LIBRARY
Console.WriteLine("Loading from media library...");
var mediaList = await LoadMedia();
var count = mediaList.Count;
if(count > 1)
{
    Console.WriteLine($"{mediaList.Count} medias found.");
}
else
{
    Console.WriteLine($"{mediaList.Count} media found.");
}
foreach(var media in mediaList)
{
    Console.WriteLine($"{media.Title}");
}
Console.WriteLine("Done loading from media library...");

// CREATE PHYSICAL MEDIA
Console.WriteLine("Creating Physical Media from Media Library...");
var physicalMedia = new Media();

foreach(var media in mediaList)
{
    Console.WriteLine($"Writing all songs from {media.Title}...");
    foreach(var song in media.Songs)
    {
        if (!IsOver60Minutes(physicalMedia))
        {
            physicalMedia.Songs.Add(song);
        }
    }
}

Console.WriteLine("Done creating physical media!");
Console.WriteLine("Physical Media track list:");
if(physicalMedia.Songs.Count > 0)
{
    Console.WriteLine("Title - Artist");
    foreach (var song in physicalMedia.Songs)
    {
        Console.WriteLine($"{song.Title} - {song.Artist}");
    }
}


Console.ReadLine();

public class Song
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public decimal Duration { get; set; }
}

public class Media
{
    public Media()
    {
        Songs = new();
    }
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public List<Song>? Songs { get; set; }
}
public class ApiResponseResource
{
    public bool Success { get; set; }
    public List<Media>? MediaResources { get; set; }
    public string? Message { get; set; }
}
