using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class VariableCollection : IEnumerable<MathVariable>
    {
        public VariableCollection()
        {
        }

        public void Add(MathVariable v)
        {
            m_dict[v.Name] = v;
        }

        public MathVariable GetOrAdd(string name)
        {
            if (!m_dict.TryGetValue(name, out MathVariable v))
            {
                v = new MathVariable(name);
                m_dict[name] = v;
            }

            return v;
        }

        public IEnumerator<MathVariable> GetEnumerator() => m_dict.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private readonly Dictionary<string, MathVariable> m_dict = new();
    }
}
