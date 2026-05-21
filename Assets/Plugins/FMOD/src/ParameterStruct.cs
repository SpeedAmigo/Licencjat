using System;

namespace FMODUnity
{
    [Serializable]
    public struct ParameterValues
    {
        public ParameterValues(string name, float value)
        {
            parameterName = name;
            this.value = value;
        }
        
        public string parameterName;
        public float value;
    }
}

