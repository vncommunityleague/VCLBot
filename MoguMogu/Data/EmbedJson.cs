using System;
using Discord;
using Newtonsoft.Json;

namespace MoguMogu.Data
{
    public class EmbedJson
    {
        [JsonProperty("content")]
        public string Content { set; get; }
        [JsonProperty("embed")]
        public Embed Embed { set; get; }
    }

    public class Embed
    {
        [JsonProperty("title")]
        public string Title { set; get; }
        [JsonProperty("description")]
        public string Description { set; get; }
        [JsonProperty("url")]
        public string Url { set; get; }

        [JsonProperty("color")] public uint Color { set; get; }
        [JsonProperty("timestamp")]
        public string Timestamp { set; get; }
        [JsonProperty("footer")]
        public Footer Footer { set; get; }
        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail { set; get; }
        [JsonProperty("image")]
        public Image Image { set; get; }
        [JsonProperty("author")]
        public Author Author { set; get; }
        [JsonProperty("fields")]
        public Field[] Fields { set; get; }
        
        public Discord.Embed GetEmbed()
        {
            var author = new EmbedAuthorBuilder
            {
                Name = Author?.Name,
                IconUrl = Author?.IconUrl,
                Url = Author?.IconUrl
            };
            var footer = new EmbedFooterBuilder
            {
                IconUrl = Footer?.IconUrl,
                Text = Footer?.Text
            };
            var builder = new EmbedBuilder
            {
                Author = author,
                Footer = footer,
                Title = Title,
                Url = Url,
                ThumbnailUrl = Thumbnail?.Url,
                ImageUrl = Image?.Url,
                Color = new Color(Color)
            };
            if (!string.IsNullOrEmpty(Timestamp))
                builder.Timestamp = DateTime.Parse(Timestamp);
            foreach (var f in Fields)
                builder.AddField(f.Name, f.Value, f.Inline);
            return builder.Build();
        }
    }

    public class Footer
    {
        [JsonProperty("icon_url")]
        public string IconUrl { set; get; }
        [JsonProperty("text")]
        public string Text { set; get; }
    }

    public class Thumbnail
    {
        [JsonProperty("url")]
        public string Url { set; get; }
    }
    public class Image
    {
        [JsonProperty("url")]
        public string Url { set; get; }
    }
    public class Author
    {
        [JsonProperty("name")]
        public string Name { set; get; }
        [JsonProperty("url")]
        public string Url { set; get; }
        [JsonProperty("icon_url")]
        public string IconUrl { set; get; }
    }

    public class Field
    {
        [JsonProperty("name")]
        public string Name { set; get; }
        [JsonProperty("value")]
        public string Value { set; get; }
        [JsonProperty("inline")]
        public bool Inline { set; get; }
    }
}