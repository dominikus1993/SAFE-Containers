namespace Catalog.Api.Model

open System
open MongoDB.Bson.Serialization.Attributes
open MongoDB.Bson

[<CLIMutable>]
[<BsonIgnoreExtraElements>]
type ProductDetails = { [<BsonElement>]  Weight: double; [<BsonElement>] WeightUnits: string; [<BsonElement>] Manufacturer: string; [<BsonElement>] Color: string }

[<CLIMutable>]
type Product = { [<BsonId>] [<BsonRepresentation(BsonType.String)>] Id: Guid;
                 [<BsonElement>] Slug: string;
                 [<BsonElement>] Name: string;
                 [<BsonElement>] Description: string;
                 [<BsonElement>] Details: ProductDetails
                 [<BsonElement>] Price: double // Decimal comparasion in dotnet use string
                 [<BsonElement>] AvailableStock: int
                 [<BsonElement>] PictureUri: string
                 [<BsonElement>] Tags: string array }
