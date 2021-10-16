using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class VariableList : IEnumerable<MathVariable>
    {
        public VariableList(MathVariable[] variables)
        {
            Variables = variables;
            VariableIndices = new Dictionary<MathVariable, int>(Variables.Length);

            for (int i = 0; i < Variables.Length; i++)
            {
                VariableIndices[Variables[i]] = i;
            }
        }

        public static implicit operator VariableList(MathVariable[] variables) => new(variables);

        private MathVariable[] Variables { get; }
        private Dictionary<MathVariable, int> VariableIndices { get; }

        public int Length => Variables.Length;
        public MathVariable this[int index] => Variables[index];
        public int this[MathVariable variable] => VariableIndices[variable];

        public IEnumerator<MathVariable> GetEnumerator() => Variables.Cast<MathVariable>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
