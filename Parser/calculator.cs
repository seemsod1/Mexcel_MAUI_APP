using Antlr4.Runtime;


namespace Parser;
    public static class Calculator
    {
        public static Table CellTable { get; }
        public static string EvaluatingCellName { get; set; }

        static Calculator()
        {
            CellTable = new Table();
            EvaluatingCellName = "";
        }
       
        

        public static double Evaluate(string expression)
        {
            var lexer = new CountingLexer(new AntlrInputStream(expression));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowExceptionErrorListener());

            var tokens = new CommonTokenStream(lexer);
            var parser = new CountingParser(tokens);

            var tree = parser.compileUnit();

            var visitor = new CountingVisitor();

            return visitor.Visit(tree);
        }

    }

