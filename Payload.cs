using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopDivoom
{
    public class Payload
    {
        public required string Command { get; set; }
        public string? FileName { get; set; }
        public int FileType { get; set; }
    }
}
