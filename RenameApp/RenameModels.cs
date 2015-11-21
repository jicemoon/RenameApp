/* ==============================
* Author: jicemoon
* QQ: 375114086
* E-mail: jicemoon@outlook.com
* Time：2015/6/26 12:09:05
* FileName：RenameModels
* Version：V0.0.0.0
* ===============================
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace RenameApp
{
    class RenameModels
    {
        #region 字段
        private static RenameModels renameModelSingle = null;
        private string modelURL = System.Environment.CurrentDirectory + @"\" + "RenameApp.data";

        public static readonly string ModelElementName = "model";
        public static readonly string ListElementName = "list";
        public static readonly string ItemElementName = "item";

        private int idIndex;
        private XmlDocument xd = null;
        private List<Model> models = null;
        private Model resetModel = null;
        private Model defaultModel = null;
        private Model lastModel = null;
        private string lastRootFolder = "";
        #endregion

        #region 属性
        /// <summary>
        /// RenameModels的单例
        /// </summary>
        public static RenameModels RenameModelSingle
        {
            get
            {
                if(renameModelSingle == null)
                    renameModelSingle = new RenameModels();
                return renameModelSingle;
            }
        }
        /// <summary>
        /// 保存模板的xml文档引用
        /// </summary>
        public XmlDocument XD
        {
            get
            {
                if(xd == null)
                {
                    initXD();
                }
                return xd;
            }
        }
        /// <summary>
        /// 当前已保存的所有模板列表
        /// </summary>
        public List<Model> Models
        {
            get
            {
                if(models == null)
                {
                    initXD();
                }
                return models;
            }
        }
        /// <summary>
        /// 默认模板, 如果没有设置, 则返回LastModel
        /// </summary>
        public Model DefaultModel
        {
            get
            {
                if(defaultModel == null)
                {
                    defaultModel = LastModel;
                }
                return defaultModel;
            }
            set
            {
                if(defaultModel.ID != value.ID)
                {
                    for(int i = 0; i < models.Count; i++)
                    {
                        if(models[i].IsDefault)
                        {
                            models[i].IsDefault = false;
                            models[i].ToXMLElement();
                            break;
                        }
                    }
                    value.IsDefault = true;
                    value.ToXMLElement();
                    saveXML();
                    defaultModel = value;
                }

            }
        }
        public Model ResetModel
        {
            get
            {
                if(resetModel == null)
                {
                    resetModel = new Model();
                    resetModel.ID = "jicemoon";
                    ModelList ml = new ModelList();
                    ml.AllItem = 2;
                    ml.Items.Add("1");
                    ml.Items.Add("0");
                    resetModel.Lists.Add(ml);
                }
                return resetModel;
            }
        }
        /// <summary>
        /// 上次退出时使用的模板(只读)
        /// </summary>
        public Model LastModel
        {
            get
            {
                if(lastModel == null)
                {
                    lastModel = ResetModel;
                }
                return lastModel;
            }
        }
        public string LastRootFolder
        {
            get
            {
                return lastRootFolder;
            }
            set
            {
                lastRootFolder = value;
                xd.DocumentElement.SetAttribute("lastRootFolder", value);
            }
        }
        #endregion

        #region 构造函数
        private RenameModels()
        {
            if(renameModelSingle != null)
            {
                return;
            }
            initXD();
        }
        #endregion

        #region 公有方法
        /// <summary>
        /// 添加模版
        /// </summary>
        /// <param name="model">要添加的模板</param>
        public void AddModel(Model model)
        {
            if(model == null)
                return;
            idIndex++;
            xd.DocumentElement.SetAttribute("index", idIndex.ToString());
            model.ID = "jicemoon_" + (idIndex);
            model.ToXMLElement();
            xd.DocumentElement.AppendChild(model.XE);
            models.Add(model);
            saveXML();
        }
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="modelID">要删除的模板id</param>
        public void RemoveModel(string modelID)
        {
            RemoveModel(GetModel(modelID));
        }
        /// <summary>
        /// 删除模版
        /// </summary>
        /// <param name="md">要删除的模板</param>
        public void RemoveModel(Model md)
        {
            if(md != null)
                models.Remove(md);
            md.XE.ParentNode.RemoveChild(md.XE);
            saveXML();
        }
        /// <summary>
        /// 保存模板文件
        /// </summary>
        public void saveXML()
        {
            File.WriteAllBytes(modelURL, encrypting(xd.OuterXml));
            //xd.Save(modelURL);
        }
        /// <summary>
        /// 根据给定的模版ID, 从模版库中查找对应的模板
        /// </summary>
        /// <param name="modelID">要查找的模板ID</param>
        /// <returns>找到的模板, 如果没有找到, 会返回null</returns>
        public Model GetModel(string modelID)
        {
            foreach(Model m in models)
            {
                if(m.ID == modelID)
                {
                    return m;
                }
            }
            return null;
        }
        /// <summary>
        /// 刷新模版
        /// </summary>
        /// <param name="md">要刷新的模板</param>
        public void refreshModel(Model md)
        {
            md.ToXMLElement();
            saveXML();
        }
        #endregion

        #region 私有方法
        //初始化xml文档
        private void initXD()
        {
            xd = new XmlDocument();
            if(File.Exists(modelURL))
            {
                //如果已经存在, 加载对应文件
                loadXML();
                idIndex = Convert.ToInt32(xd.DocumentElement.GetAttribute("index"));
                lastRootFolder = xd.DocumentElement.GetAttribute("lastRootFolder");
                models = new List<Model>();
                XmlNodeList modelXNL = xd.DocumentElement.GetElementsByTagName(ModelElementName);
                foreach(XmlNode modelNode in modelXNL)
                {
                    Model md = new Model((XmlElement)modelNode);
                    if(md.IsDefault)
                        defaultModel = md;
                    if(md.ID == "jicemoon")
                    {
                        lastModel = md;
                        continue;
                    }
                    models.Add(md);
                }
            }
            else
            {
                //如果不存在,新建一个空的xml文档, 备用
                xd.AppendChild(xd.CreateXmlDeclaration("1.0", "utf-8", null));
                XmlElement root = xd.CreateElement("root");
                root.SetAttribute("index", "0");
                idIndex = 0;
                xd.AppendChild(root);
                models = new List<Model>();
            }
        }
        //加载保存模板的xml文件
        private void loadXML()
        {
            xd.LoadXml(deciphering(File.ReadAllBytes(modelURL)));
            //xd.Load(modelURL);
        }
        //简单的字符串加密
        private byte[] encrypting(string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            //return byteArray;
            List<byte> lb = new List<byte>();
            for(int i = 0; i < byteArray.Length; i++)
            {
                lb.Add((byte)(byte.MaxValue - byteArray[i]));
            }
            return lb.ToArray();
        }
        //解密二进制
        private string deciphering(byte[] byteArray)
        {
            //return Encoding.UTF8.GetString(byteArray);
            List<byte> lb = new List<byte>();
            for(int i = 0; i < byteArray.Length; i++)
            {
                lb.Add((byte)(byte.MaxValue - byteArray[i]));
            }
            return Encoding.UTF8.GetString(lb.ToArray());
        }
        #endregion

        #region 事件监听

        #endregion

    }
}
