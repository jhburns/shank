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

    exception private LexerException of string

    type private LexerHelper(source: string) =
        let mutable tokens = new ResizeArray<Token>()

        let mutable start = 0
        let mutable current = 0

        member this.LexAll() : Result<Token List, string> =
            try
                while this.IsAtEnd() |> not do
                    start <- current
                    this.LexToken()

                this.AddToken(EndOfFile)
                Ok(List.ofSeq (tokens))
            with
            | LexerException details -> Error(details)

        member private this.LexToken() : unit =
            match this.Advance() with
            | '(' -> this.AddToken(LeftParenthesis)
            | ')' -> this.AddToken(RightParenthesis)
            | '{' -> this.AddToken(LeftBrace)
            | '}' -> this.AddToken(RightBrace)
            // Continue on any whitespace
            | ' '
            | '\r'
            | '\n'
            | '\t' -> ()
            | '"' -> this.LexString()
            | c when System.Char.IsLetter(c) -> this.LexIdentifierOrKeyword()
            | c -> this.ThrowFailure(c.ToString())

        member private this.LexString() : unit =
            while (this.Peek() <> '"') && (this.IsAtEnd() |> not) do
                this.Advance() |> ignore

            if this.IsAtEnd() then
                this.ThrowFailure("end of file")
            else
                // Advance past the end quite
                this.Advance() |> ignore

                // Trim the surrounding quotes
                this.AddToken(String(source.[start + 1..current - 2]))

        member private this.LexIdentifierOrKeyword() : unit =
            let keywords = Map [ ("fn", KeyWordFn) ]

            while System.Char.IsLetterOrDigit(this.Peek()) do
                this.Advance() |> ignore

            let text = source.[start..current - 1]

            // If the text is a keyword add that,
            // Otherwise add identifier
            keywords.TryFind text
            |> Option.defaultValue (Identifier(text))
            |> this.AddToken

        member private this.Advance() : char =
            let oldCurrent = current
            current <- current + 1
            source.[oldCurrent]

        member private this.MatchChar(wanted: char) : bool =
            if this.IsAtEnd() then
                false
            else if source.[current] <> wanted then
                false
            else
                current <- current + 1
                true

        member private this.Peek() : char =
            if this.IsAtEnd() then
                '\x00'
            else
                source.[current]

        member private this.PeekNext() : char =
            if current + 1 >= String.length source then
                '\x00'
            else
                source.[current + 1]

        member private this.IsAtEnd() : bool = current >= (String.length source)

        member private this.ThrowFailure(s: string) =
            raise ($"Unexpected token '{s}'" |> LexerException)

        member private this.AddToken(tt: TokenType) : unit = tokens.Add(newToken (tt))

    let stringToTokens (source: string) : Result<Token List, string> = LexerHelper(source).LexAll()
