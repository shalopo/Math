using System;
using System.Diagnostics;

namespace MathUtil.Parsing
{
    public class MathParseContext
    {
        public MathParseContext(VariableCollection variables = null) => Variables = variables ?? new VariableCollection();

        public VariableCollection Variables { get; }
    }

    public class MathParser
    {
        public static MathExpr Parse(string input, MathParseContext context = null)
        {
            return new MathParser(input, context ?? new MathParseContext()).DoParse();
        }

        private MathParser(string input, MathParseContext context) => m_tokenizer = new MathTokenizer(input, context);

        private MathExpr DoParse()
        {
            try
            {
                return ReadClause(until_end: true);
            }
            catch (MathParseException ex)
            {
                ex.Offset = m_tokenizer.CurrentOffset;
                throw;
            }
        }

        private MathExpr ReadClause(bool until_end = false)
        {
            TokenTag expected_close = TokenTag.NONE;

            if (!until_end)
            {
                Debug.Assert(m_tokenizer.Peep().Tag == TokenTag.OPEN_BRACKET);
                expected_close = TokenTag.CLOSE_BRACKET;
                m_tokenizer.Pop();
            }

            var expr = ReadFullTerm();

            while (m_tokenizer.Peep().Tag != TokenTag.NONE && m_tokenizer.Peep().Tag != TokenTag.CLOSE_BRACKET)
            {
                var opToken = PeepOpToken();

                m_tokenizer.Pop();

                expr = opToken.Op switch
                {
                    OpType.PLUS => expr + ReadFullTerm(),
                    OpType.MINUS => expr - ReadFullTerm(),
                    _ => throw new MathParseException($"Unexpected input")
                };
            }

            //TODO: 1/xy should be parsed as: 1/(x*y), but 1/2x => (1/2)*x  because 2 is a number

            var close_token = m_tokenizer.Peep();

            if (close_token.Tag != expected_close)
            {
                throw new MathParseException("Unexpected clause closing");
            }

            m_tokenizer.Pop();

            return expr;
        }
        
        private MathExpr ReadFullTerm()
        {
            if (PeepOpToken()?.Op == OpType.MINUS)
            {
                m_tokenizer.Pop();
                return -ReadFullTerm();
            }

            var expr = ReadConsecutivePowers();

            OpToken opToken;

            while ((opToken = PeepOpToken()) != null)
            {
                if (opToken.Op == OpType.PLUS || opToken.Op == OpType.MINUS)
                {
                    return expr;
                }

                if (!opToken.IsVirtual)
                {
                    m_tokenizer.Pop();
                }

                expr = opToken.Apply(expr, ReadConsecutivePowers());
            }

            return expr;
        }

        private MathExpr ReadConsecutivePowers()
        {
            var expr = ReadNearestTerm();

            if (PeepOpToken()?.Op == OpType.POWER)
            {
                m_tokenizer.Pop();
                return expr.Pow(ReadConsecutivePowers());
            }
            else
            {
                return expr;
            }
        }

        private OpToken PeepOpToken()
        {
            var token = m_tokenizer.Peep();

            return token.Tag switch
            {
                TokenTag.OP => (OpToken)token,
                TokenTag.OPERAND => OpToken.MULTIPLY_VIRTUAL,
                TokenTag.OPEN_BRACKET => OpToken.MULTIPLY_VIRTUAL,
                TokenTag.CLOSE_BRACKET => null,
                TokenTag.NONE => null,
                _ => throw new MathParseException($"Unexpected input"),
            };
            ;
        }

        private MathExpr ReadNearestTerm()
        {
            var token = m_tokenizer.Peep();

            switch (token.Tag)
            {
                case TokenTag.OPEN_BRACKET: return ReadClause();
                case TokenTag.OPERAND: 
                    var expr = (m_tokenizer.Pop() as OperandToken).Expr;

                    if (expr is FunctionCallMathExpr call)
                    {
                        var func = call.Func;
                        var input = ReadNearestTerm();
                        return func.Call(input);
                    }

                    return expr;

                case TokenTag.OP: throw new MathParseException($"Unexpected op");
                case TokenTag.NONE: throw new MathParseException($"Unexpected termination");
                default: throw new MathParseException($"Unexpected input");
            }
        }

        readonly MathTokenizer m_tokenizer;
    }
}
