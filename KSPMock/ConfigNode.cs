using System;
using System.Collections.Generic;
using System.Linq;

namespace KSPMock
{
    public class ConfigNode
    {
        public string name;
        protected readonly Dictionary<string, string> _values = new();
        protected readonly Dictionary<string, List<ConfigNode>> _nodes = new();

        public ConfigNode()
        {

        }

        public ConfigNode(string name)
        {
            this.name = name;
        }

        public void SetValue(string key, string value)
        {
            _values[key] = value;
        }

        public string GetValue(string key)
        {
            return _values.TryGetValue(key, out var value) ? value : null;
        }

        public void AddValue(string key, object value)
        {
            _values[key] = value.ToString();
        }

        public bool TryGetValue(string key, ref string value)
        {
            return _values.TryGetValue(key, out value);
        }

        public void AddNode(string key, ConfigNode node)
        {
            if (_nodes.TryGetValue(key, out var nodeList))
            {
                nodeList.Add(node);
            }
            else
            {
                _nodes[key] = new List<ConfigNode> { node };
            }
        }

        public ConfigNode GetNode(string key)
        {
            return _nodes.TryGetValue(key, out var nodeList) ? nodeList.FirstOrDefault() : null;
        }

        public ConfigNode[] GetNodes(string key)
        {
            return _nodes.TryGetValue(key, out var nodeList) ? nodeList.ToArray() : Array.Empty<ConfigNode>();
        }

        // does not exist, just for tests
        [Obsolete]
        public IReadOnlyDictionary<string, string> __GetValueDictionary()
        {
            return this._values;
        }

        // does not exist, just for tests
        [Obsolete]
        public IReadOnlyDictionary<string, IReadOnlyList<ConfigNode>> __GetNodeDictionary()
        {
            return this._nodes.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<ConfigNode>)kvp.Value
            );
        }
    }
}
