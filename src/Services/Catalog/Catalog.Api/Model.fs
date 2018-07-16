namespace Catalog.Api.Model 

open System
[<CLIMutable>]
type Product = { Id: Guid; 
                 Slug: string;
                 Name: string;
                 Price: decimal
                 AvailableStock: int
                 PictureUri: string }