using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Parsing
{
    class MathToken
    {
        protected MathToken(TokenTag tag) => Tag = tag;

        public TokenTag Tag { get; }

        public static readonly MathToken END = new MathToken(TokenTag.NONE);
    }

    enum TokenTag
    {
        NONE,
        OPERAND,
        OP,
        OPEN_BRACKET,
        CLOSE_BRACKET,
    }

    class OpToken : MathToken
    {
        private OpToken(OpType op) : base(TokenTag.OP) => Op = op;

        public OpType Op { get; }
        public bool IsVirtual { get; set; } = false;

        public static OpToken PLUS = new OpToken(OpType.PLUS);
        public static OpToken MINUS = new OpToken(OpType.MINUS);
        public static OpToken MULTIPLY = new OpToken(OpType.MULTIPLY);
        public static OpToken MULTIPLY_VIRTUAL = new OpToken(OpType.MULTIPLY) { IsVirtual = true };
        public static OpToken DIVIDE = new OpToken(OpType.DIVIDE);
        public static OpToken POWER = new OpToken(OpType.POWER);
    }

    enum OpType
    {
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE,
        POWER,
    }

    class BracketToken : MathToken
    {
        private BracketToken(bool open) : base(open ? TokenTag.OPEN_BRACKET : TokenTag.CLOSE_BRACKET) { }

        public static BracketToken OPEN = new BracketToken(true);
        public static BracketToken CLOSE = new BracketToken(false);
    }

    class OperandToken : MathToken
    {
        public OperandToken(MathExpr expr) : base(TokenTag.OPERAND) => Expr = expr;

        public MathExpr Expr { get; }
    }

}
