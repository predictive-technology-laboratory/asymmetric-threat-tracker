using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTL.ATT.GUI.Plugins
{
    public abstract class Plugin
    {
        public abstract string MenuItemName { get; }

        public abstract void Run(AttForm att);
    }
}
