namespace Core

module Parser =

    type Identifier = Identifier of string

    type Expression =
        | FunctionCall of callee: Expression * arg: Expression // print("Hello")
        | StringLiteral of value: string // "example"
        | ExpressionId of id: Identifier // print

    type TopLevel = Function of id: Identifier * body: Expression List // fn main() { ... }

    type Ast = TopLevel List

    exception private ParserException of string

    exception private UnreachableException

    type private ParserHelper(tokens: Lexer.Tokens) =
        let mutable current = 0

        member this.ParseAll() : Result<Ast, string> =
            try
                let topLevels = new ResizeArray<TopLevel>()

                while this.IsAtEnd() |> not do
                    topLevels.Add(this.TopLevel())

                Ok(List.ofSeq topLevels)

            with
            | ParserException details -> Error(details)

        member private this.TopLevel() : TopLevel =
            if (this.MatchTags([ Lexer.TagKeyWordFn ])) then
                this.Function()
            else
                raise (ParserException "Excepted a function declaration at top level")

        member private this.Function() : TopLevel =
            let ident =
                this.ConsumeId("Expected an identifier to start function declaration")

            this.Consume(Lexer.TagLeftParenthesis, "Expect left parenthesis in function parameters")
            |> ignore

            this.Consume(Lexer.TagRightParenthesis, "Expected right parenthesis in function paramers")
            |> ignore

            this.Consume(Lexer.TagLeftBrace, "Expected start of a block")
            |> ignore

            let statements = this.Block()

            Function(ident, statements)

        member private this.Block() : Expression List =
            let statements = new ResizeArray<Expression>()

            while this.Check(Lexer.TagRightBrace) |> not
                  && this.IsAtEnd() |> not do
                statements.Add(this.ExpressionStatement())

            this.Consume(Lexer.TagRightBrace, "Expected closing brace at end of functio")
            |> ignore

            List.ofSeq (statements)

        member private this.ExpressionStatement() : Expression =
            let expr = this.Expression()

            this.Consume(Lexer.TagSeperator, "Expcted a semicolon after an expression statement")
            |> ignore

            expr

        member private this.Expression() : Expression = this.Call()

        member private this.Call() : Expression =
            let mutable expr = this.Primary()

            while this.MatchTags([ Lexer.TagLeftParenthesis ]) do
                let arg = this.Expression()

                this.Consume(Lexer.TagRightParenthesis, "Expected function call to have closing parenthesis")
                |> ignore

                expr <- FunctionCall(expr, arg)

            expr

        member private this.Primary() : Expression =
            if this.MatchTags([ Lexer.TagString ]) then
                match this.Previous().Type with
                | Lexer.String str -> (StringLiteral(str))
                | _ -> raise UnreachableException
            else if this.MatchTags([ Lexer.TagIdentifier ]) then
                match this.Previous().Type with
                | Lexer.Identifier id -> ExpressionId(Identifier(id))
                | _ -> raise UnreachableException
            else
                raise (ParserException "Unexpected token")

        member private this.MatchTags(tags: Lexer.TokenTag List) : bool =
            if List.exists this.Check tags then
                this.Advance() |> ignore
                true
            else
                false

        member private this.Consume(tag: Lexer.TokenTag, errorMsg: string) : Lexer.Token =
            if this.Check(tag) then
                this.Advance()
            else
                raise (ParserException errorMsg)

        member private this.ConsumeId(errorMsg: string) : Identifier =
            match this.Consume(Lexer.TagIdentifier, errorMsg).Type with
            | Lexer.Identifier (id) -> Identifier(id)
            | _ -> raise UnreachableException

        member private this.Check(tag: Lexer.TokenTag) : bool =
            if this.IsAtEnd() then
                false
            else
                this.Peek().Type
                |> Lexer.toTokenTag
                |> Lexer.tokenTagEq tag

        member private this.Advance() : Lexer.Token =
            if this.IsAtEnd() |> not then
                current <- current + 1
                this.Previous()
            else
                this.Previous()

        member private this.IsAtEnd() : bool =
            match this.Peek().Type with
            | Lexer.EndOfFile -> true
            | _ -> false

        member private this.Peek() : Lexer.Token = tokens [ current ]

        member private this.Previous() : Lexer.Token = tokens [ current - 1 ]

    let tokensToAst (tokens: Lexer.Tokens) : Result<Ast, string> = ParserHelper(tokens).ParseAll()
