﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SpaceNews.Scraper.Reader;

public class SourceEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public required string Name { get; set; }
    public required string Url { get; set; }
}
