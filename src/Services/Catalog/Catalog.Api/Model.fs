namespace Catalog.Api.Model

open System
open MongoDB.Bson.Serialization.Attributes
open MongoDB.Bson

[<CLIMutable>]
type Product = { [<BsonId>] [<BsonRepresentation(BsonType.String)>] Id: Guid;
                 [<BsonElement>] Slug: string;
                 [<BsonElement>] Name: string;
                 [<BsonElement>] Price: decimal
                 [<BsonElement>] AvailableStock: int
                 [<BsonElement>] PictureUri: string }
