using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Data;


namespace DirectoryPositioner {
    class DataSource {
        static DataSource() {
            CreateSourceFile();
        }

        const String SRC_FILE_NAME = "data.xml";

        public static bool CreateSourceFile() {
            if( !File.Exists( SRC_FILE_NAME ) ) {
                var fs = File.Create( SRC_FILE_NAME );
                using( var sw = new StreamWriter( fs ) ) {
                    var xmlString = @"<?xml version='1.0' encoding='utf-8'?>";
                    xmlString += "<Paths></Paths>";
                    sw.Write( xmlString );
                    sw.Flush();
                }
                return true;
            }
            return false;
        }


        private static XmlElement GetElementByPath( XmlDocument doc, string path ) {
            var eles = doc.GetElementsByTagName( "Path" );
            var result = eles.Cast<XmlElement>().Where( item => item.InnerText == path );
            return result.FirstOrDefault();
        }

        public static bool AddPath( string path, string name ) {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load( SRC_FILE_NAME );
            if( GetElementByPath( xmlDoc, path ) != null ) {
                return false;
            }
            var newPath = xmlDoc.CreateElement( "Path" );
            newPath.InnerText = path;
            newPath.SetAttribute( "Name", name );
            var paths = xmlDoc.GetElementsByTagName( "Paths" )[ 0 ];
            paths.AppendChild( newPath );
            xmlDoc.Save( SRC_FILE_NAME );
            return true;
        }

        public static bool EditPath( string oldPath, string path, string name ) {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load( SRC_FILE_NAME );
            var ele = GetElementByPath( xmlDoc, oldPath );
            if( ele == null ) {
                return false;
            }
            ele.InnerText = path;
            ele.SetAttribute( "Name", name );
            xmlDoc.Save( SRC_FILE_NAME );
            return true;
        }

        public static DataTable GetPathList() {
            try {
                var dataSet = new DataSet();
                dataSet.ReadXml( SRC_FILE_NAME );
                return dataSet.Tables[ "Path" ];
            } catch( Exception ) {
                return new DataTable();
            }
        }

        public static List<ConfigItem> GetDataList() {
            Func<XObject, string> getValue = node => {
                if( node == null ) {
                    return string.Empty;
                }
                if( node is XAttribute ) {
                    return ( node as XAttribute ).Value;
                } else if( node is XText ) {
                    return ( node as XText ).Value;
                }
                return string.Empty;
            };

            var xDoc = XDocument.Load( SRC_FILE_NAME );
            var result = from ele in xDoc.Descendants( "Path" )
                         orderby getValue( ele.Attribute( "Name" ) ) ascending
                         select new ConfigItem {
                             Name = getValue( ele.Attribute( "Name" ) ),
                             BgColor = getValue( ele.Attribute( "BgColor" ) ),
                             TextColor = getValue( ele.Attribute( "TextColor" ) ),
                             Path = getValue( ele.FirstNode as XText )
                         };
            return result.ToList();
        }

        public static bool Delete( string path ) {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load( SRC_FILE_NAME );
            var ele = GetElementByPath( xmlDoc, path );
            if( ele == null )
                return false;
            ele.ParentNode.RemoveChild( ele );
            xmlDoc.Save( SRC_FILE_NAME );
            return true;
        }

        public static bool SetBgColor( string path, string color ) {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load( SRC_FILE_NAME );
            var ele = GetElementByPath( xmlDoc, path );
            if( ele == null ) {
                return false;
            }
            ele.SetAttribute( "BgColor", color );
            xmlDoc.Save( SRC_FILE_NAME );
            return true;
        }

        public static bool SetTextColor( string path, string color ) {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load( SRC_FILE_NAME );
            var ele = GetElementByPath( xmlDoc, path );
            if( ele == null ) {
                return false;
            }
            ele.SetAttribute( "TextColor", color );
            xmlDoc.Save( SRC_FILE_NAME );
            return true;
        }

        private const string PAGE_MODE_BTN = "Btn";
        private const string PAGE_MODE_LIST = "List";

        public static PageMode GetPageMode() {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load( SRC_FILE_NAME );
            var pageModeString = xmlDoc.DocumentElement.GetAttribute( "Mode" );
            if( string.IsNullOrEmpty( pageModeString ) ) {
                return PageMode.List;
            } else if( pageModeString == PAGE_MODE_LIST ) {
                return PageMode.List;
            } else if( pageModeString == PAGE_MODE_BTN ) {
                return PageMode.Btn;
            } else {
                return PageMode.List;
            }
        }

        public static void SetPageMode( PageMode pageMode ) {
            var pageModeString = PAGE_MODE_LIST;
            if( pageMode == PageMode.Btn ) {
                pageModeString = PAGE_MODE_BTN;
            }
            var xmlDoc = new XmlDocument();
            xmlDoc.Load( SRC_FILE_NAME );
            xmlDoc.DocumentElement.SetAttribute( "Mode", pageModeString );
            xmlDoc.Save( SRC_FILE_NAME );
        }
    }
}
