using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace DirectoryPositioner {
    class ConfigItem {
        public string Name {
            get;set;
        }

        public string BgColor {
            get; set;
        }

        public string TextColor {
            get; set;
        }

        public string Path {
            get; set;
        }


        public string Type {
            get;set;
        }

        public int OpenCount {
            get;set;
        }
    }
}
