using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArchipelagoRandomizer
{
    [AttributeUsage(AttributeTargets.Method)]
    public class APAttribute : Attribute
    {
        public string CommandName { get; }
        public string[] CommandAliases { get; }

        public APAttribute(string commandName, string[] commandAliases)
        {
            CommandName = commandName;
            CommandAliases = commandAliases;
        }

    }
}
