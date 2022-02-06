namespace Core

module Lexer =

    type TokenType =
        | KeyWordFn // fn
        | Identifier of string // example0
        | LeftParenthesis // (
        | RightParenthesis // )
        | LeftBrace // {
        | RightBrace // }
        | String of string // "example"
        | EndOfFile

    type Token = { Type: TokenType }

    let private newToken (t: TokenType) : Token = { Type = t }

    exception LexerFailure of string

    type private LexerHelper(source: string) =
        let mutable tokens = new ResizeArray<Token>()

        let mutable start = 0
        let mutable current = 0

        member this.LexAll() : Result<ResizeArray<Token>, string> =
            try
                while this.isAtEnd () |> not do
                    start <- current
                    this.lexToken ()

                this.addToken (EndOfFile)
                Ok(tokens)
            with
            | LexerFailure details -> Error(details)

        member this.lexToken() : unit =
            match this.advance () with
            | '(' -> this.addToken (LeftParenthesis)
            | ')' -> this.addToken (RightParenthesis)
            | '{' -> this.addToken (LeftBrace)
            | '}' -> this.addToken (RightBrace)
            // Continue on any whitespace
            | ' '
            | '\r'
            | '\n'
            | '\t' -> ()
            | '"' -> this.lexString ()
            | c when System.Char.IsLetter(c) -> this.lexIdentifierOrKeyword ()
            | c -> this.throwFailure (c.ToString())

        member this.lexString() : unit =
            while (this.peek () <> '"') && (this.isAtEnd () |> not) do
                this.advance () |> ignore

            if this.isAtEnd () then
                this.throwFailure ("end of file")
            else
                // Advance past the end quite
                this.advance () |> ignore

                // Trim the surrounding quotes
                this.addToken (String(source.[start + 1..current - 2]))

        member this.lexIdentifierOrKeyword() : unit =
            let keywords = Map [ ("fn", KeyWordFn) ]

            while System.Char.IsLetterOrDigit(this.peek ()) do
                this.advance () |> ignore

            let text = source.[start..current - 1]

            // If the text is a keyword add that,
            // Otherwise add identifier
            keywords.TryFind text
            |> Option.defaultValue (Identifier(text))
            |> this.addToken

        member private this.advance() : char =
            let oldCurrent = current
            current <- current + 1
            source.[oldCurrent]

        member private this.matchChar(wanted: char) : bool =
            if this.isAtEnd () then
                false
            else if source.[current] = wanted |> not then
                false
            else
                current <- current + 1
                true

        member private this.peek() : char =
            if this.isAtEnd () then
                '\x00'
            else
                source.[current]

        member private this.peekNext() : char =
            if current + 1 >= String.length source then
                '\x00'
            else
                source.[current + 1]

        member private this.isAtEnd() : bool = current >= (String.length source)

        member private this.throwFailure(s: string) =
            raise (sprintf "Unexpected token '%s'" s |> LexerFailure)

        member private this.addToken(tt: TokenType) : unit = tokens.Add(newToken (tt))

    let stringToTokens (source: string) : Result<ResizeArray<Token>, string> = (new LexerHelper(source)).LexAll()
