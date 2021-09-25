using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaturnBot.Entities
{

    public class PartialUser
    {
        public string username { get; set; }
        public ulong id { get; set; }
        public PartialUser()
        {

        }
    }
    public class PartialGuild
    {
        public string name { get; set; }
        public ulong id { get; set; }
        public bool owner { get; set; }
        public uint permissions { get; set; }
    }
}
