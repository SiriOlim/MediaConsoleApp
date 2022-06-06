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

Song GetNextSong(Media media, int index)
{
    return media?.Songs[index] ?? null;
}

Media WriteSongToMedia(Media media, Song song)
{
    media.Songs.Add(song);
    return media;
}

List<Media> FinalizeMedia(List<Media> writeToPhysicalMedias, List<Media> mediaLibrary)
{
    var physicalMedia = new Media();
    physicalMedia.Title = "PhysicalMedia#1";
    foreach (var media in mediaLibrary)
    {
        Console.WriteLine($"Writing all songs from {media.Title}...");
        for (int x = 0; x < media.Songs.Count; x++)
        {
            var nextSong = GetNextSong(media, x);
            if (nextSong != null)
            {
                if(!IsOver60Minutes(physicalMedia, nextSong))
                {
                    WriteSongToMedia(physicalMedia, nextSong);
                }
                else
                {
                    writeToPhysicalMedias.Add(physicalMedia);
                    physicalMedia = new Media();
                    physicalMedia.Title = $"PhysicalMedia#{writeToPhysicalMedias.Count + 1}";
                    WriteSongToMedia(physicalMedia, nextSong);
                }
            }
        }
    }
    writeToPhysicalMedias.Add(physicalMedia);
    return writeToPhysicalMedias;
}

bool IsOver60Minutes(Media currentMedia, Song nextSong)
{
    var isOver60 = false;
    isOver60 = (currentMedia?.Songs?.Sum(x => x.Duration) + nextSong.Duration) > 60;
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
Console.WriteLine("Writing to Physical Media from Media Library...");
var physicalMedia = new List<Media>();

FinalizeMedia(physicalMedia, mediaList);

Console.WriteLine("Done creating physical media!");
Console.WriteLine("Physical Media track list:");
if (physicalMedia.Count > 0)
{
    foreach (var media in physicalMedia)
    {
        Console.WriteLine($"{media.Title}");
        if (media.Songs.Count > 0)
        {
            Console.WriteLine("Title - Artist - Duration");
            foreach (var song in media.Songs)
            {
                Console.WriteLine($"{song.Title} - {song.Artist} - {song.Duration}");
            }
        }
        Console.WriteLine($"Total duration - {media.Songs.Sum(x => x.Duration)}");
        Console.WriteLine("");
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
