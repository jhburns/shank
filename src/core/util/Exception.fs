namespace Core

// Common exceptions used throughout the Core
module Exception =
    // `Data0` is the implicitly generated property for the exception string
    exception UnreachableException of string with
        override this.Message = this.Data0

    let unreachable (message: string) : 'never =
        message
        |> sprintf "Unreachable path taken '%s'"
        |> Unreachable
        |> raise
