module Catalog.Types


type ProductDetails = { weight: double; weightUnits: string; manufacturer: string; color: string }

type Product = { id: string;
                 slug: string;
                 name: string;
                 description: string;
                 details: ProductDetails
                 price: double
                 availableStock: int
                 pictureUri: string
                 tags: string array }

type Model = { Products: Product array; Page: int; PageSize: int; ErrorMessage: string option; Loading: bool }

type Msg =
  | BrowseProducts of page: int * pageSize: int
  | FetchedProducts of Product array
  | FetchError of exn
