namespace Catalog.Api.Model

open System
open MongoDB.Bson.Serialization.Attributes
open MongoDB.Bson

[<CLIMutable>]
[<BsonIgnoreExtraElements>]
type ProductDetails = { [<BsonElement>]  Weight: float; [<BsonElement>] WeightUnits: string; [<BsonElement>] Manufacturer: string; [<BsonElement>] Color: string }

[<CLIMutable>]
type Product = { [<BsonId>] [<BsonRepresentation(BsonType.String)>] Id: Guid;
                 [<BsonElement>] Slug: string;
                 [<BsonElement>] Name: string;
                 [<BsonElement>] Description: string;
                 [<BsonElement>] Details: ProductDetails
                 [<BsonElement>] Price: decimal
                 [<BsonElement>] AvailableStock: int
                 [<BsonElement>] PictureUri: string }
