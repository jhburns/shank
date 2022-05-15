#nowarn "0025"


[<EntryPoint>]
let main _args =

    let (Ok output) =
        Core.Lexer.stringToTokens
            """
    fn main() {
        print("hello world");
    }
    """

    Core.Parser.tokensToAst output |> printfn "%A"

    0
