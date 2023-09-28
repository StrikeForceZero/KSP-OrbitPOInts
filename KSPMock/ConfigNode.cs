using System;

namespace KSPMock
{
    public class ConfigNode
    {
        public string name;

        public ConfigNode()
        {

        }

        public ConfigNode(string name)
        {
            this.name = name;
        }

        public void SetValue(string key, string value)
        {
            throw new NotImplementedException();
        }

        public string GetValue(string key)
        {
            throw new NotImplementedException();
        }

        public void AddValue(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, ref string value)
        {
            throw new NotImplementedException();
        }

        public void AddNode(string key, ConfigNode node)
        {
            throw new NotImplementedException();
        }

        public ConfigNode GetNode(string key)
        {
            throw new NotImplementedException();
        }

        public ConfigNode[] GetNodes(string key)
        {
            throw new NotImplementedException();
        }
    }
}
