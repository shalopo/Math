using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MathUtil.Parsing
{
    class MathTokenizer
    {
        public MathTokenizer(string input, MathParseContext context) => (m_input, m_context) = (input, context);

        private static readonly Regex S_RGX_TOKEN = new(
            @"\G\s*(?:" +
            @"(?<bracket>[()])|" +
            @"(?<op>[+\-*/^])|" +
            @"(?<num>[0-9][0-9,]*(?:\.[0-9]+)?)|" +
            @$"(?<func>{string.Join("|", GlobalMathDefs.Functions.Keys)})|" +
            @"(?<const>[eiπ]|pi)|" +
            @"(?<var>[a-z][0-9]*)" +
            @")\s*", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] S_GROUP_NAMES = S_RGX_TOKEN.GetGroupNames().
            Where(n => !int.TryParse(n, out var i)).ToArray();

        public MathToken Pop()
        {
            if (Peep() == null)
            {
                return null;
            }

            Debug.Assert(m_peep_length >= 0);
            m_current_offset += m_peep_length;
            m_peep_length = -1;

            var lastPeep = m_peep;
            m_peep = null;

            return lastPeep;
        }

        public MathToken Peep()
        {
            if (m_peep != null)
            {
                return m_peep;
            }

            var (token, length) = GetNextToken();

            m_peep = token;
            m_peep_length = length;

            return m_peep;
        }

        private (MathToken, int) GetNextToken()
        {
            if (m_current_offset == m_input.Length)
            {
                return (MathToken.END, 0);
            }

            var match = S_RGX_TOKEN.Match(m_input, m_current_offset);

            if (!match.Success)
            {
                throw new MathParseException("Unexpected token");
            }

            return (ParseMatch(match), match.Length);
        }

        private MathToken ParseMatch(Match match)
        {
            foreach (var groupName in S_GROUP_NAMES)
            {
                var group = match.Groups[groupName];

                if (group.Success)
                {
                    var value = group.Captures[0].Value;

                    return groupName switch
                    {
                        "bracket" => ParseBracket(value),
                        "op" => ParseOp(value),
                        "num" => ParseNumber(value),
                        "const" => ParseConst(value),
                        "var" => ParseVar(value),
                        "func" => ParseFunc(value),
                        _ => null
                    };
                }
            }

            throw new MathParseException("Internal error", new NotImplementedException($"No matching group"));
        }

        private OpToken ParseOp(string input)
        {
            return input switch
            {
                "+" => OpToken.PLUS,
                "-" => OpToken.MINUS,
                "*" => OpToken.MULTIPLY,
                "/" => OpToken.DIVIDE,
                "^" => OpToken.POWER,
                _ => throw new MathParseException($"Unexpected input {input}")
            };
        }

        private BracketToken ParseBracket(string input)
        {
            return input switch
            {
                "(" => BracketToken.OPEN,
                ")" => BracketToken.CLOSE,
                _ => throw new MathParseException($"Unexpected input {input}")
            };
        }

        private OperandToken ParseNumber(string input)
        {
            if (!double.TryParse(input, out double number))
            {
                throw new MathParseException($"Failed to parse number: {input}");
            }

            return new OperandToken(new ExactConstMathExpr(number));
        }
        
        private OperandToken ParseConst(string input)
        {
            return new OperandToken(input.ToLower() switch
            {
                "e" => GlobalMathDefs.E,
                "pi" => GlobalMathDefs.PI,
                "π" => GlobalMathDefs.PI,
                "i" => GlobalMathDefs.I,
                _ => throw new MathParseException($"Unexpected input {input}")
            });
        }

        private OperandToken ParseVar(string input)
        {
            return new OperandToken(m_context.Variables.GetOrAdd(input));
        }

        private OperandToken ParseFunc(string input)
        {
            var name = input.ToLower();

            if (GlobalMathDefs.Functions.TryGetValue(name, out MathFunctionDef f))
            {
                return new OperandToken(f.Call(new MathVariable("_")));
            }

            throw new MathParseException("Internal error", new NotImplementedException($"No matching function"));
        }

        public int CurrentOffset => m_current_offset;

        readonly string m_input;
        readonly MathParseContext m_context;

        int m_current_offset = 0;
        MathToken m_peep = null;
        int m_peep_length = -1;
    }

}
