using SaturnBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaturnBot.CommandModules
{
    interface Module
    {
        List<Command> Load();
    }
}
