module Catalog.Types

type ProductFilter =
  | PriceMin of priceMin: decimal
  | PriceMax of priceMax: decimal
  | Name of name: string

type ProductDetails = { weight: double; weightUnits: string; manufacturer: string; color: string }

type Product = { id: string;
                 slug: string;
                 name: string;
                 brand: string;
                 description: string;
                 details: ProductDetails
                 price: double
                 availableStock: int
                 pictureUri: string
                 tags: string array }

type Model = { Products: Product array; Page: int; PageSize: int; TotalItems: int; TotalPages: int; ErrorMessage: string option; Sort: string; Loading: bool }

type PagedProducts = { products: Product seq; totalItems: int; totalPages: int }

type PagedMeta = { page: int; totalItems: int; totalPages: int }

type ProductData<'a, 'b> = { data: 'a; metadata: 'b }

type Msg =
  | BrowseProducts of page: int * pageSize: int * sort: string * filters: ProductFilter array
  | FetchedProducts of ProductData<Product array, PagedMeta>
  | FetchError of exn
