using System.Diagnostics;

namespace MathUtil.Parsing
{
    public class MathParseContext
    {
        public MathParseContext(VariableCollection variables) => Variables = variables;

        public VariableCollection Variables { get; }
    }

    public class MathParser
    {
        public static MathExpr Parse(string input, MathParseContext context)
        {
            return new MathParser(input, context).DoParse();
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
            var expr = ReadNearestTerm();

            OpToken opToken;

            while ((opToken = PeepOpToken()) != null)
            {
                var new_expr = opToken.Op switch
                {
                    OpType.PLUS => expr,
                    OpType.MINUS => expr,
                    OpType.MULTIPLY => expr * ConsumeOpAndReadNext(opToken),
                    OpType.DIVIDE => expr / ConsumeOpAndReadNext(opToken),
                    OpType.POWER => expr.Pow(ConsumeOpAndReadNext(opToken)),
                    _ => throw new MathParseException($"Unexpected input")
                };

                if (new_expr == expr)
                {
                    return expr;
                }

                expr = new_expr;
            }

            return expr;

            MathExpr ConsumeOpAndReadNext(OpToken opToken)
            {
                if (!opToken.IsVirtual)
                {
                    m_tokenizer.Pop();
                }

                return ReadNearestTerm();
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

            return token.Tag switch
            {
                TokenTag.OPEN_BRACKET => ReadClause(),
                TokenTag.OPERAND => (m_tokenizer.Pop() as OperandToken).Expr,
                TokenTag.NONE => throw new MathParseException($"Unexpected termination"),
                _ => throw new MathParseException($"Unexpected input")
            };
        }

        readonly MathTokenizer m_tokenizer;
    }
}
