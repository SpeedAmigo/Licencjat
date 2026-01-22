using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Commands
{
    public class CommandsManager : MonoBehaviour
    {
        public static CommandsManager Instance;
        [SerializeField] private TMP_InputField textField;

        private Dictionary<string, MethodInfo> _commands = new();
        private Dictionary<Type, object> _instances = new();
        private string _input;

        public void RegisterInstance(object instance)
        {
            _instances.Add(instance.GetType(), instance);
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assebly in assemblies)
            {
                foreach (MethodInfo methodInfo in assebly.GetTypes().SelectMany(classType => classType.GetMethods()))
                {
                    var attributes = methodInfo.GetCustomAttributes<CommandAttribute>().ToList();
                    if (attributes.Count == 0) continue;

                    foreach (CommandAttribute attribute in attributes)
                    {
                        Debug.Log($"{attribute.CommandName} | {methodInfo.Name}");
                        _commands.Add(attribute.CommandName, methodInfo);
                    }
                }
            }

            textField.onSubmit.AddListener(OnSubmit);
        }

        private void OnSubmit(string text)
        {
            _input = text;
            ProcessCommands();
            _input = "";
            textField.text = "";
        }

        private void ProcessCommands()
        {
            string[] tokens = _input.Split(" ");
            string[] parameterTokens = tokens.Skip(1).ToArray();
            if (tokens.Length == 0) return;

            if (!_commands.TryGetValue(tokens[0], out var methodInfo))
            {
                Debug.LogError($"Command \"{tokens[0]}\" doesnt exist");
                return;
            }

            ParameterInfo[] parametersInfos = methodInfo.GetParameters();
            
            if(parametersInfos.Length != parameterTokens.Length)
            {
                Debug.LogError("Wrong number of parameters");
                return;
            }
            
            List<object> invocationParams = new List<object>();

            for (int i = 0; i < parametersInfos.Length; i++)
            {
                var parameterInfo = parametersInfos[i];
                invocationParams.Add(Convert.ChangeType(parameterTokens[i], parameterInfo.ParameterType));
            }

            object instance = this;


            if(methodInfo.DeclaringType != null && _instances.ContainsKey(methodInfo.DeclaringType))
            {
                instance = _instances[methodInfo.DeclaringType];
            }

            methodInfo.Invoke(instance, invocationParams.ToArray());
        }

    }
}
