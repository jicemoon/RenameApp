/* ==============================
* Author: jicemoon
* QQ: 375114086
* E-mail: jicemoon@outlook.com
* Time：2015/6/26 12:29:16
* FileName：ModelList
* Version：V0.0.0.0
* ===============================
*/
using System;
using System.Collections.Generic;
using System.Xml;

namespace RenameApp
{
    public class Model
    {
        #region 字段
        private string id = "";
        private string name = "";
        private bool isDefault = false;
        private List<ModelList> lists = null;
        private XmlElement xe;
        #endregion

        #region 属性
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public bool IsDefault{
            get
            {
                return isDefault;
            }
            set
            {
                isDefault = value;
            }
        }
        public List<ModelList> Lists
        {
            get
            {
                if(lists == null)
                    lists = new List<ModelList>();
                return lists;
            }
        }
        public XmlElement XE
        {
            get
            {
                return xe;
            }
            set
            {
                xe = value;
            }
        }
        #endregion

        #region  构造方法
        public Model()
        {
            
        }
        public Model(XmlElement modelNode)
        {
            xe = modelNode;
            name = modelNode.GetAttribute("name");
            id = modelNode.GetAttribute("id");
            isDefault = modelNode.GetAttribute("isDefault") == "1"?true:false;
            XmlNodeList listXNL = modelNode.GetElementsByTagName(RenameModels.ListElementName);
            foreach(XmlNode listNode in listXNL)
            {
                Lists.Add(new ModelList((XmlElement)listNode));
            }
        }
        #endregion

        #region 公有方法
        public void ToXMLElement()
        {
            if(xe == null)
                xe = RenameModels.RenameModelSingle.XD.CreateElement(RenameModels.ModelElementName);
            else
            {
                xe.RemoveAll();
            }
            xe.SetAttribute("id", id);
            xe.SetAttribute("isDefault", isDefault?"1":"0");
            xe.SetAttribute("name", name);
            int length = lists.Count;
            for(int i = 0; i < length; i++)
            {
                lists[i].ToXMLElement();
                xe.AppendChild(lists[i].XE);
            }
        }
        public bool IsEqual(Model md)
        {
            if(lists.Count == md.lists.Count)
            {
                for(int i = 0; i < lists.Count; i++)
                {
                    if(lists[i].AllItem == md.lists[i].AllItem)
                    {
                        for(int j = 0; j < lists[i].Items.Count; j++)
                        {
                            if(lists[i].Items[j] != md.lists[i].Items[j])
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        #endregion

        #region 私有方法

        #endregion

        #region 事件监听

        #endregion
    }
    public class ModelList
    {
        #region 字段
        private int allItem = 0;
        private List<string> items = null;
        private XmlElement xe;
        #endregion

        #region 属性
        public int AllItem
        {
            get{
                return allItem;
            }
            set
            {
                allItem = value;
            }
        }
        public List<string> Items
        {
            get{
                if(items == null)
                    items = new List<string>();
                return items;
            }
        }
        public XmlElement XE
        {
            get
            {
                return xe;
            }
            set
            {
                xe = value;
            }
        }
        #endregion

        #region  构造方法
        public ModelList()
        {
        
        }
        public ModelList(XmlElement listNode)
        {
            xe = listNode;
            allItem = Convert.ToInt32(listNode.GetAttribute("allItem"));
            foreach(XmlNode xn0 in listNode.GetElementsByTagName(RenameModels.ItemElementName))
            {
                Items.Add(xn0.InnerText);
            }
        }
        #endregion

        #region 公有方法
        public void ToXMLElement()
        {
            if(xe == null)
            {
                xe = RenameModels.RenameModelSingle.XD.CreateElement(RenameModels.ListElementName);
            }
            else
            {
                xe.RemoveAll();
            }
            xe.SetAttribute("allItem", allItem.ToString());
            int length = items.Count;
            for(int i = 0; i < length; i++)
            {
                XmlElement item;
                item = RenameModels.RenameModelSingle.XD.CreateElement(RenameModels.ItemElementName);
                item.InnerText = items[i];
                xe.AppendChild(item);
            }
        }
        #endregion

        #region 私有方法

        #endregion

        #region 事件监听

        #endregion
    }
}
