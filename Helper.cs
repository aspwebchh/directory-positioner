using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pinyin4net;
using Pinyin4net.Format;
using System.Text.RegularExpressions;
using System.IO;

namespace DirectoryPositioner {
    class Helper {
        public static string GetInitials( string chineseString ) {
            if( string.IsNullOrWhiteSpace( chineseString ) ) {
                return string.Empty;
            }

            Func<char,string> getPinYin = c => {
                string[] pinYin = PinyinHelper.ToHanyuPinyinStringArray( c );
                if( pinYin == null ) {
                    return c.ToString();
                }
                var one = pinYin[ 0 ];
                return one[0].ToString();
            };
            var result = chineseString.ToCharArray().Select( getPinYin ).ToArray();
            return string.Join( "", result );
        }

        public static string GetPinYin( string chineseString ) {
            if( string.IsNullOrWhiteSpace( chineseString ) ) {
                return string.Empty;
            }
            Func<char, string> getPinYin = c => {
                HanyuPinyinOutputFormat format = new HanyuPinyinOutputFormat();
                format.ToneType = HanyuPinyinToneType.WITHOUT_TONE;
                string[] pinYin = PinyinHelper.ToHanyuPinyinStringArray( c , format );
                if( pinYin == null ) {
                    return c.ToString();
                }
                var one = pinYin[ 0 ];
                return one;
            };
            var result = chineseString.ToCharArray().Select( getPinYin ).ToArray();
            return string.Join( "", result );
        }


        public const string ITEM_TYPE_LINK = "链接";
        public const string ITEM_TYPE_DIR = "目录";
        public const string ITEM_TYPE_FILE = "文件";
        public const string ITEM_TYPE_UNKOWN = "未知";

        public static string JudgePathType(string path) {
            if( Regex.IsMatch( path, @"^http(s)?://", RegexOptions.IgnoreCase ) ) {
                return ITEM_TYPE_LINK;
            } else if( Directory.Exists(path) ) {
                return ITEM_TYPE_DIR;
            } else if( File.Exists(path) ) {
                return ITEM_TYPE_FILE;
            } else {
                return ITEM_TYPE_UNKOWN;
            }
        }
    }
}
