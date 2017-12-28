using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;


namespace fast_open_work_dir
{
    class DataSource
    {
        static DataSource()
        {
            CreateSourceFile();
        }

        const String SRC_FILE_NAME = "data.xml";

        public static bool CreateSourceFile()
        {
            if (!File.Exists(SRC_FILE_NAME))
            {
                var fs = File.Create(SRC_FILE_NAME);
                using (var sw = new StreamWriter(fs))
                {
                    var xmlString = @"<?xml version='1.0' encoding='utf-8'?>";
                    xmlString += "<Paths></Paths>";
                    sw.Write(xmlString);
                    sw.Flush();
                }
                return true;
            }
            return false;
        }


        private static XmlElement GetElementByPath(XmlDocument doc, string path)
        {
            var eles = doc.GetElementsByTagName("Path");
            var result = eles.Cast<XmlElement>().Where(item => item.InnerText == path);
            return result.FirstOrDefault();
        }

        public static bool AddPath(string path, string name)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(SRC_FILE_NAME);
            if (GetElementByPath(xmlDoc, path) != null)
            {
                return false;
            }
            var newPath = xmlDoc.CreateElement("Path");
            newPath.InnerText = path;
            newPath.SetAttribute("Name", name);
            var paths = xmlDoc.GetElementsByTagName("Paths")[0];
            paths.AppendChild(newPath);
            xmlDoc.Save(SRC_FILE_NAME);
            return true;
        }

        public static DataTable GetPathList()
        {
            try
            {
                var dataSet = new DataSet();
                dataSet.ReadXml(SRC_FILE_NAME);
                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }

        public static bool Delete(string path )
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(SRC_FILE_NAME);
            var ele = GetElementByPath(xmlDoc, path);
            if (ele == null) return false;
            ele.ParentNode.RemoveChild(ele);
            xmlDoc.Save(SRC_FILE_NAME);
            return true;
        }
    }
}
