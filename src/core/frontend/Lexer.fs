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
        | Seperator // ;
        | EndOfFile

    // This type exists so the Parser can check the type of a token
    type TokenTag =
        | TagKeyWordFn
        | TagIdentifier
        | TagLeftParenthesis
        | TagRightParenthesis
        | TagLeftBrace
        | TagRightBrace
        | TagString
        | TagSeperator
        | TagEndOfFile

    type Token = { Type: TokenType }

    let private newToken (t: TokenType) : Token = { Type = t }

    let toTokenTag (token: TokenType) : TokenTag =
        match token with
        | KeyWordFn -> TagKeyWordFn
        | Identifier _ -> TagIdentifier
        | LeftParenthesis -> TagLeftParenthesis
        | RightParenthesis -> TagRightParenthesis
        | LeftBrace -> TagLeftBrace
        | RightBrace -> TagRightBrace
        | String _ -> TagString
        | Seperator -> TagSeperator
        | EndOfFile -> TagEndOfFile

    let tokenTagEq (tt1: TokenTag) (tt2: TokenTag) : bool =
        match (tt1, tt2) with
        | (TagKeyWordFn, TagKeyWordFn)
        | (TagIdentifier, TagIdentifier)
        | (TagLeftParenthesis, TagLeftParenthesis)
        | (TagRightParenthesis, TagRightParenthesis)
        | (TagLeftBrace, TagLeftBrace)
        | (TagRightBrace, TagRightBrace)
        | (TagString, TagString)
        | (TagSeperator, TagSeperator)
        | (TagEndOfFile, TagEndOfFile) -> true
        | _ -> false

    type Tokens = Token List

    exception private LexerException of string

    type private LexerHelper(source: string) =
        let tokens = ResizeArray<Token>()

        let mutable start = 0
        let mutable current = 0

        member this.LexAll() : Result<Token List, string> =
            try
                while this.IsAtEnd() |> not do
                    start <- current
                    this.LexToken()

                this.AddToken(EndOfFile)
                Ok(List.ofSeq tokens)
            with
            | LexerException details -> Error(details)

        member private this.LexToken() : unit =
            match this.Advance() with
            | '(' -> this.AddToken(LeftParenthesis)
            | ')' -> this.AddToken(RightParenthesis)
            | '{' -> this.AddToken(LeftBrace)
            | '}' -> this.AddToken(RightBrace)
            | ';' -> this.AddToken(Seperator)

            // Continue on any other whitespace
            | '\n'
            | ' '
            | '\r'
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
                // Advance past the end quotes
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

        member private this.Peek() : char =
            if this.IsAtEnd() then
                '\x00'
            else
                source.[current]

        member private this.IsAtEnd() : bool = current >= (String.length source)

        member private this.ThrowFailure(s: string) =
            raise ($"Unexpected token '{s}'" |> LexerException)

        member private this.AddToken(tt: TokenType) : unit = tokens.Add(newToken (tt))

    let stringToTokens (source: string) : Result<Token List, string> = LexerHelper(source).LexAll()
