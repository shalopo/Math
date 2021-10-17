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

        public VariableCollection AsReadOnly()
        {
            VariableCollection newCollection = new();
            
            foreach (var v in this)
            {
                newCollection.Add(v);
            }

            newCollection.IsReadOnly = true;

            return newCollection;
        }

        public void Add(MathVariable v)
        {
            m_dict[v.Name] = v;
        }

        public bool Contains(string name) => m_dict.ContainsKey(name);

        public MathVariable this[string name]
        {
            get => m_dict[name];
            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Variable collection is readonly");
                }

                m_dict[name] = value;
            }
        }

        public MathVariable GetOrAdd(string name)
        {
            if (!m_dict.TryGetValue(name, out MathVariable v))
            {
                v = new MathVariable(name);
                this[name] = v;
            }

            return v;
        }

        public IEnumerator<MathVariable> GetEnumerator() => m_dict.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private readonly Dictionary<string, MathVariable> m_dict = new();

        public bool IsReadOnly { get; private set; }
    }
}
