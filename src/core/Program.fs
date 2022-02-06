#nowarn "0025"

[<EntryPoint>]
let main _args =
    let (Ok output) =
        Core.Lexer.stringToTokens
            """
    fn main() {
        print("hello world")
    }
    """

    output
    |> List.ofSeq
    |> List.map (sprintf "%A")
    |> String.concat ",\n"
    |> printfn "%s"
    |> ignore

    0
