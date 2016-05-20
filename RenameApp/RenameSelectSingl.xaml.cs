using System;
using System.Windows;
using System.Windows.Controls;

namespace RenameApp
{
    /// <summary>
    /// RenameSelectSingl.xaml 的交互逻辑
    /// </summary>
    public partial class RenameSelectSingl:UserControl
    {
        #region 字段
        private static int rsNum = 0;
        /// <summary>
        /// 指示是否在用已保存的模版初始化
        /// </summary>
        private bool isInit = false;
        private ModelList currentML = null;
        #endregion

        #region 属性
        public delegate void RefreshRequest(object sender, EventArgs e, RenameSelectSingl newRSS = null);
        /// <summary>
        /// 列表项内容有更改时, 触发此事件
        /// </summary>
        public event RefreshRequest onRefreshRequest;
        /// <summary>
        /// <para>当前选择的种类, 数字说明如下:</para>
        /// <list>
        /// <para>0: 文本</para>
        /// <para>1: 新扩展名</para>
        /// <para>2: 当前文件名</para>
        /// <para>3: 序列数字</para>
        /// <para>4: 序列字母</para>
        /// <para>5: 时间日期</para>
        /// <para>6: 文件夹名称</para>
        /// <para>7: 字符串替换</para>
        /// </list>
        /// </summary>
        public int TypeUint
        {
            get
            {
                return allItems.SelectedIndex;
            }
            set
            {
                if(value != allItems.SelectedIndex)
                {
                    displayOrHide((allItems.SelectedItem as ComboBoxItem).Name, true);
                    allItems.SelectedIndex = value;
                    displayOrHide((allItems.SelectedItem as ComboBoxItem).Name, false);
                }
            }
        }
        public ModelList CurrentML
        {
            get
            {
                readCurrentML();
                return currentML;

            }
        }
        #endregion

        #region 构造方法
        public RenameSelectSingl()
        {
            InitializeComponent();
            this.Loaded += delegate
            {
                rsNum++;
                renameBefore.GroupName = "replaceRadios_" + rsNum;
                renameAfter.GroupName = "replaceRadios_" + rsNum;
            };
        }
        #endregion

        #region 公共方法
        public void resetByModelList(ModelList ml, Panel parent)
        {
            isInit = true;
            resetUpDownBtn(parent);
            allItems.SelectedIndex = ml.AllItem;
            switch(ml.AllItem)
            {
                case 0:
                    //文本
                    if(ml.Items.Count != 1)
                        break;
                    textSelected.Text = ml.Items[0];
                    break;
                case 1:
                    //扩展名
                    if(ml.Items.Count != 2)
                        break;
                    extension1Selected.Text = ml.Items[0];
                    extension2Selected.IsChecked = (ml.Items[1] == "1");
                    break;
                case 2:
                    //当前文件名
                    if(ml.Items.Count != 2)
                        break;
                    nowName1Selected.SelectedIndex = Convert.ToInt32(ml.Items[0]);
                    nowName2Selected.SelectedIndex = Convert.ToInt32(ml.Items[1]);
                    break;
                case 3:
                    //序列数字
                    if(ml.Items.Count != 2)
                        break;
                    orderNum1Selected.Text = ml.Items[0];
                    orderNum2Selected.SelectedIndex = Convert.ToInt32(ml.Items[1]);
                    break;
                case 4:
                    //序列字母
                    if(ml.Items.Count != 1)
                        break;
                    orderLetterSelected.SelectedIndex = Convert.ToInt32(ml.Items[0]);
                    break;
                case 5:
                    //时间日期
                    if(ml.Items.Count != 2)
                        break;
                    time1Selected.SelectedIndex = Convert.ToInt32(ml.Items[0]);
                    time2Selected.SelectedIndex = Convert.ToInt32(ml.Items[1]);
                    break;
                case 6:
                    //文件夹名称
                    if(ml.Items.Count != 1)
                        break;
                    folderNameSelected.SelectedIndex = Convert.ToInt32(ml.Items[0]);
                    break;
                case 7:
                    //字符串替换
                    if(ml.Items.Count != 7)
                        break;
                    replaceStr3Selected.IsChecked = (ml.Items[0] == "1");
                    replaceStr4Selected.IsChecked = (ml.Items[1] == "1");
                    searchString.Text = ml.Items[2];
                    replaceString.Text = ml.Items[3];
                    isIgnoreCase.IsChecked = (ml.Items[4] == "1");
                    if(ml.Items[5] == "1")
                        renameBefore.IsChecked = true;
                    else
                        renameAfter.IsChecked = true;
                    useRegular.IsChecked = (ml.Items[6] == "1");
                    break;
                default:
                    return;
            }
            isInit = false;
            refreshRequestMethod(this);
        }
        /// <summary>
        /// 将当前重命名项转化为xmlElement, 并添加到参数xml文档中
        /// </summary>
        /// <param name="root"></param>
        private void readCurrentML()
        {
            if(IsLoaded)
            {
                currentML = new ModelList();
                currentML.AllItem = allItems.SelectedIndex;

                switch(currentML.AllItem)
                {
                    case 0:
                        //文本
                        currentML.Items.Add(textSelected.Text);
                        break;
                    case 1:
                        //扩展名
                        currentML.Items.Add(extension1Selected.Text);
                        currentML.Items.Add(extension2Selected.IsChecked == true ? "1" : "0");
                        break;
                    case 2:
                        //当前文件名
                        currentML.Items.Add(nowName1Selected.SelectedIndex.ToString());
                        currentML.Items.Add(nowName2Selected.SelectedIndex.ToString());
                        break;
                    case 3:
                        //序列数字
                        currentML.Items.Add(orderNum1Selected.Text);
                        currentML.Items.Add(orderNum2Selected.SelectedIndex.ToString());
                        break;
                    case 4:
                        //序列字母
                        currentML.Items.Add(orderLetterSelected.SelectedIndex.ToString());
                        break;
                    case 5:
                        //时间日期
                        currentML.Items.Add(time1Selected.SelectedIndex.ToString());
                        currentML.Items.Add(time2Selected.SelectedIndex.ToString());
                        break;
                    case 6:
                        //文件夹名称
                        currentML.Items.Add(folderNameSelected.SelectedIndex.ToString());
                        break;
                    case 7:
                        //字符串替换
                        currentML.Items.Add(replaceStr3Selected.IsChecked == true ? "1" : "0");
                        currentML.Items.Add(replaceStr4Selected.IsChecked == true ? "1" : "0");
                        currentML.Items.Add(searchString.Text);
                        currentML.Items.Add(replaceString.Text);
                        currentML.Items.Add(isIgnoreCase.IsChecked == true ? "1" : "0");
                        currentML.Items.Add(renameBefore.IsChecked == true ? "1" : "0");
                        currentML.Items.Add(useRegular.IsChecked == true ? "1" : "0");
                        break;
                    default:
                        return;
                }
            }
            else
                currentML = null;
        }
        private System.Xml.XmlElement proItem(string value, System.Xml.XmlDocument root)
        {
            System.Xml.XmlElement item;
            item = root.CreateElement("item");//, textSelected.Text
            item.InnerText = value;
            return item;
        }
        #endregion

        #region 已注释
        //public
        /// <summary>
        /// <para>当前选择的种类, 字符串说明如下:</para>
        /// <list>
        /// <para>"txt": 文本</para>
        /// <para>"extension": 新扩展名</para>
        /// <para>"nowName": 当前文件名</para>
        /// <para>"orderNum": 序列数字</para>
        /// <para>"orderLetter": 序列字母</para>
        /// <para>"time": 时间日期</para>
        /// <para>"folderName": 文件夹名称</para>
        /// <para>"replaceStr": 字符串替换</para>
        /// </list>
        /// </summary>
        //public string typeStr
        //{
        //    get
        //    {
        //        return (allItems.SelectedItem as ComboBoxItem).Name;
        //    }
        //    set
        //    {
        //        for(int i = 0; i < allItems.Items.Count; i++)
        //        {
        //            if((allItems.Items[i] as ComboBoxItem).Name == value)
        //            {
        //                allItems.SelectedIndex = i;
        //                break;
        //            }
        //        }
        //    }
        //}
        #endregion

        #region 私有方法
        //显示或隐藏对应重命名方法对应的参数选项
        private void displayOrHide(string key, bool isHide = false)
        {
            switch(key)
            {
                case "text":
                    //文本
                    if(isHide)
                        textSelected.Visibility = System.Windows.Visibility.Hidden;
                    else
                        textSelected.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "extension":
                    //新扩展名
                    if(isHide)
                    {
                        extension1Selected.Visibility = System.Windows.Visibility.Hidden;
                        extension2Selected.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        extension1Selected.Visibility = System.Windows.Visibility.Visible;
                        extension2Selected.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case "nowName":
                    //当前文件名
                    if(isHide)
                    {
                        nowName1Selected.Visibility = System.Windows.Visibility.Hidden;
                        nowName2Selected.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        nowName1Selected.Visibility = System.Windows.Visibility.Visible;
                        nowName2Selected.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case "orderNum":
                    //序列数字
                    if(isHide)
                    {
                        orderNum1Selected.Visibility = System.Windows.Visibility.Hidden;
                        orderNum2Selected.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        orderNum1Selected.Visibility = System.Windows.Visibility.Visible;
                        orderNum2Selected.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case "orderLetter":
                    //序列字母
                    if(isHide)
                    {
                        orderLetterSelected.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        orderLetterSelected.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case "time":
                    //时间日期
                    if(isHide)
                    {
                        time1Selected.Visibility = System.Windows.Visibility.Hidden;
                        time2Selected.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        time1Selected.Visibility = System.Windows.Visibility.Visible;
                        time2Selected.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case "folderName":
                    //文件夹名称
                    if(isHide)
                    {
                        folderNameSelected.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        folderNameSelected.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case "replaceStr":
                    //字符串替换
                    if(isHide)
                    {
                        replaceStr1Selected.Visibility = System.Windows.Visibility.Collapsed;
                        replaceStr2Selected.Visibility = System.Windows.Visibility.Collapsed;
                        replaceStr3Selected.Visibility = System.Windows.Visibility.Hidden;
                        replaceStr4Selected.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        replaceStr1Selected.Visibility = System.Windows.Visibility.Visible;
                        replaceStr2Selected.Visibility = System.Windows.Visibility.Visible;
                        replaceStr3Selected.Visibility = System.Windows.Visibility.Visible;
                        replaceStr4Selected.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
            }
        }
        //重置向上/向下按钮是否可用
        private void resetUpDownBtn(Panel par)
        {
            int length = par.Children.Count;
            if(length > 0)
            {
                for(int i = 0; i < length; i++)
                {
                    (par.Children[i] as RenameSelectSingl).upBtn.IsEnabled = true;
                    (par.Children[i] as RenameSelectSingl).downBtn.IsEnabled = true;
                }
                (par.Children[0] as RenameSelectSingl).upBtn.IsEnabled = false;
                (par.Children[par.Children.Count - 1] as RenameSelectSingl).downBtn.IsEnabled = false;
            }

        }
        private void refreshRequestMethod(RenameSelectSingl rss = null)
        {
            if(onRefreshRequest != null)
            {
                onRefreshRequest(this, new EventArgs(), rss);
            }
        }
        #endregion

        #region 事件监听处理
        //列表项中第一个选项改变时调用;
        private void allItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(IsLoaded)
            {
                try
                {
                    ComboBoxItem removedItem = e.RemovedItems[0] as ComboBoxItem;
                    ComboBoxItem addedItem = e.AddedItems[0] as ComboBoxItem;
                    displayOrHide(removedItem.Name, true);
                    displayOrHide(addedItem.Name, false);
                    refreshRequestMethod();
                }
                catch(Exception)
                {
                    //throw;
                }
            }

        }
        //删除此重命名单元
        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if(IsLoaded)
            {
                StackPanel par = this.Parent as StackPanel;
                if(par != null)
                {
                    par.Children.Remove(this);
                    if(par.Children.Count == 1)
                    {
                        (par.Children[0] as RenameSelectSingl).deleteBtn.IsEnabled = false;
                    }
                    resetUpDownBtn(par);
                    refreshRequestMethod();
                }
            }
        }

        //在当前单元的下面添加一个新的重命名单元
        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            if(IsLoaded)
            {
                StackPanel par = this.Parent as StackPanel;
                if(par != null)
                {
                    if(par.Children.Count == 1)
                    {
                        (par.Children[0] as RenameSelectSingl).deleteBtn.IsEnabled = true;
                    }
                    RenameSelectSingl rss = new RenameSelectSingl();
                    rss.deleteBtn.IsEnabled = true;
                    par.Children.Insert(par.Children.IndexOf(this) + 1, rss);
                    resetUpDownBtn(par);
                    refreshRequestMethod(rss);
                }
            }

        }

        //向上移动
        private void upBtn_Click(object sender, RoutedEventArgs e)
        {
            if(IsLoaded)
            {
                StackPanel par = this.Parent as StackPanel;
                if(par != null)
                {
                    int index = par.Children.IndexOf(this);
                    par.Children.RemoveAt(index);
                    par.Children.Insert(index - 1, this);
                    resetUpDownBtn(par);
                    refreshRequestMethod();
                }
            }
        }
        //向下移动
        private void downBtn_Click(object sender, RoutedEventArgs e)
        {
            if(IsLoaded)
            {
                StackPanel par = this.Parent as StackPanel;
                if(this.IsLoaded && par != null)
                {
                    int index = par.Children.IndexOf(this);
                    par.Children.RemoveAt(index);
                    par.Children.Insert(index + 1, this);
                    resetUpDownBtn(par);
                    refreshRequestMethod();
                }
            }

        }

        //文本框内容文本改变时调用
        private void onTextBox_TextChangedHandle(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if(!isInit && this.IsLoaded && tb != null && tb.Visibility == System.Windows.Visibility.Visible
                && (tb.Parent as Panel) != null && (tb.Parent as Panel).Visibility == System.Windows.Visibility.Visible)
            {
                refreshRequestMethod();
            }
        }
        //下拉列表选项改变时调用
        private void onComboBoxSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if(!isInit && this.IsLoaded && cb != null && cb.Visibility == System.Windows.Visibility.Visible && (cb.Parent as Panel).Visibility == System.Windows.Visibility.Visible)
                refreshRequestMethod();
        }
        //复选框是否选中改变时调用
        private void onCheckBoxCheckedHandle(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if(!isInit && this.IsLoaded && cb != null && cb.Visibility == System.Windows.Visibility.Visible && (cb.Parent as Panel).Visibility == System.Windows.Visibility.Visible)
                refreshRequestMethod();
        }
        //字符串替换选项中的单选按钮, 选择事件
        private void onReplaceRadioButtonCheckedHandle(object sender, RoutedEventArgs e)
        {
            if(!isInit && this.IsLoaded && replaceStr2Selected.Visibility == System.Windows.Visibility.Visible)
                refreshRequestMethod();
        }
        #endregion
    }
}
