﻿using Newtonsoft.Json;

namespace ReflectBlog.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ImgurResponseModel
    {
        public int Status { get; set; }
        public bool Success { get; set; }

        public ImgurImage Data { get; set; }

    }


    /// <summary>
    /// 
    /// </summary>
    public class ImgurImage
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("datetime")]
        public int Datetime { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("animated")]
        public bool Animated { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("views")]
        public long Views { get; set; }
        [JsonProperty("bandwidth")]
        public long Bandwidth { get; set; }
        [JsonProperty("deletehash")]
        public string Deletehash { get; set; }
        [JsonProperty("section")]
        public object Section { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
