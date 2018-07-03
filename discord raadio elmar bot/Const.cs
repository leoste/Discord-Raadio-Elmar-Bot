using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace discord_raadio_elmar_bot
{
    static class Const
    {
        public static string Token {
            get
            {
                return File.ReadAllText("token.txt");
            }
        }
        public static readonly string radio = "http://striiming.trio.ee/elmar.aac";
    }
}
