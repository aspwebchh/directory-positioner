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

        public static string JudgePathType(string path) {
            if( Regex.IsMatch( path, @"^http(s)?://", RegexOptions.IgnoreCase ) ) {
                return "链接";
            } else if( Directory.Exists(path) ) {
                return "目录";
            } else if( File.Exists(path) ) {
                return "文件";
            } else {
                return "未知";
            }
        }
    }
}
