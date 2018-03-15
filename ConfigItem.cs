using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace DirectoryPositioner {
    class ConfigItem {
        public String Name {
            get;set;
        }

        public String BgColor {
            get; set;
        }

        public String TextColor {
            get; set;
        }

        public String Path {
            get; set;
        }


        public string Type {
            get;set;
        }
    }
}
