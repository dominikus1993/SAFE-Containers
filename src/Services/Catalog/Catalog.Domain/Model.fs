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
                 [<BsonElement>] Brand: string;
                 [<BsonElement>] Description: string;
                 [<BsonElement>] Details: ProductDetails
                 [<BsonElement>] Price: double // Decimal comparasion in dotnet use string
                 [<BsonElement>] AvailableStock: int
                 [<BsonElement>] PictureUri: string
                 [<BsonElement>] Tags: string array }
with
  static member Zero() =
    { Id = Guid.NewGuid()
      Slug = ""
      Name = ""
      Brand = ""
      Description = ""
      Details = { Weight = 0.; WeightUnits = ""; Manufacturer = ""; Color = "" }
      Price = 0.
      AvailableStock = 0
      PictureUri = ""
      Tags = [||]
    }

type PagedProducts = { Products: Product seq; TotalItems: int; TotalPages: int }

type PagedMeta = { Page: int; TotalItems: int; TotalPages: int }

type Data<'a, 'b> = { Data: 'a; Metadata: 'b }
