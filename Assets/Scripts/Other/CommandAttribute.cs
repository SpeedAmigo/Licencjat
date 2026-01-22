using System;

namespace Commands 
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {

        public readonly string CommandName;
        public readonly string CommandDeescription;
        
        public CommandAttribute(string commandName, string commandDescription)
        {
            CommandName = commandName;
            CommandDeescription = commandDescription;
        }
    }

}
