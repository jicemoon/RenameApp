using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;

namespace RenameApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow:Window
    {
        private readonly FolderBrowserDialog browserFolder = new FolderBrowserDialog();
        private List<string> renameFolderRoot;
        private List<string> fileLists = new List<string>();
        private PreviewWindow previewWindow = null;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        #region 加载和保存xml模板
        private void resetRenameListByModel(string modelID)
        {
            resetRenameListByModel(RenameModels.RenameModelSingle.GetModel(modelID));
        }
        private void resetRenameListByModel(Model md)
        {
            if(IsLoaded && md != null)
            {
                renameListsStackPanel.Children.Clear();
                int length = md.Lists.Count;
                for(int i = 0; i < length; i++)
                {
                    int tempI = i;
                    RenameSelectSingl rss = new RenameSelectSingl();
                    renameListsStackPanel.Children.Add(rss);
                    rss.Loaded += delegate
                    {
                        rss.onRefreshRequest += onRenameSelectSingleRequestHandle;
                        rss.resetByModelList(md.Lists[tempI], renameListsStackPanel);
                        if(length > 1)
                            rss.deleteBtn.IsEnabled = true;
                    };
                }
            }
        }
        private Model convertToModel(string name)
        {
            if(name == null || name.Length <= 0)
                return null;
            Model rtn = new Model();
            rtn.Name = name;
            return convertToModel(rtn);
        }
        private Model convertToModel(Model md)
        {
            md.Lists.Clear();
            int length = renameListsStackPanel.Children.Count;
            for(int i = 0; i < length; i++)
            {
                RenameSelectSingl rss = renameListsStackPanel.Children[i] as RenameSelectSingl;
                if(rss != null)
                {
                    md.Lists.Add(rss.CurrentML);
                }
            }
            return md;
        }
        private void addToModelComboBox(Model md)
        {
            ComboBoxItem cbi = new ComboBoxItem();
            cbi.Name = md.ID;
            cbi.Content = md.Name;

            selectMoble.Items.Insert(selectMoble.Items.Count - 1, cbi);
            selectMoble.SelectedIndex = selectMoble.Items.Count - 2;
        }
        private void removeFromModelComboBox(string modelID)
        {
            int length = selectMoble.Items.Count;
            for(int i = 0; i < length; i++)
            {
                ComboBoxItem cbi = selectMoble.Items[i] as ComboBoxItem;
                if(cbi.Name == modelID)
                {
                    selectMoble.Items.RemoveAt(i);
                    break;
                }
            }
        }
        private void removeModifyItems()
        {
            if(IsLoaded)
            {
                for(int i = 0; i < selectMoble.Items.Count; i++)
                {
                    ComboBoxItem cbi = selectMoble.Items[i] as ComboBoxItem;
                    if(cbi == null)
                    {
                        selectMoble.Items.RemoveAt(i);
                        i--;
                    }
                    else if((cbi.Content as string).IndexOf("(已修改)") > -1)
                    {
                        selectMoble.Items.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        #endregion

        #region 私有方法
        private void refreshFiles(string patter)
        {
            fileListBox.Items.Clear();
            fileLists.Clear();
            int length = renameFolderRoot.Count;
            for(int i = 0; i < length; i++)
            {
                traversalFold(renameFolderRoot[i], fileLists, patter, (bool)isContainerSons.IsChecked);
            }
            if(!(bool)isContainerHideFiles.IsChecked)
                removeHideFiles();
            addToFileListBox(fileListBox, fileLists);
            refreshPreviewLabel();
            isForbidSomeButtons(false);
        }
        //将文件加入到文件显示列表
        private void addToFileListBox(System.Windows.Controls.ListBox fileListBox, List<string> fileLists)
        {
            int length = fileLists.Count;
            for(int i = 0; i < length; i++)
            {
                //fileListBox.Items.Add(System.IO.Path.GetFileName(fileLists[i]));
                fileListBox.Items.Add(fileLists[i]);
            }
            fileListTile.Content = "文件列表(共 " + length + " 个文件)";
        }
        /// <summary>
        /// 遍历文件夹内的所有文件
        /// </summary>
        /// <param name="fold">需要遍历的文件夹</param>
        /// <param name="files">将遍历到的文件列表输出到此List对象中</param>
        private void traversalFold(string fold, List<string> files, string patter, bool hasSon = true)
        {
            if(patter == null || patter.Length <= 0)
            {
                List<string> tl = Directory.GetFiles(fold).ToList();
                tl.Sort(new FilesNameComparerClass());
                files.AddRange(tl);
            }
            else
                files.AddRange(Directory.GetFiles(fold, patter).Where(t => t.ToLower().EndsWith(patter.Replace(@"*", ""))).ToList());
            if(!hasSon)
                return;
            string[] tempfold = Directory.GetDirectories(fold);

            foreach(string next in tempfold)
            {
                traversalFold(next, files, patter);
            }
        }

        //刷新文件type列表
        private void refreshFileType()
        {
            int length = fileLists.Count;
            List<string> types = new List<string>();
            string temp = "";
            bool isHas;
            for(int i = 0; i < length; i++)
            {
                temp = System.IO.Path.GetExtension(fileLists[i]).ToLower();
                isHas = types.Contains(temp);
                if(!isHas)
                {
                    types.Add(temp);
                }
            }
            fileType.Items.Clear();
            fileType.Items.Add("所有");
            for(int i = 0; i < types.Count; i++)
            {
                fileType.Items.Add(@"*" + types[i]);
            }
            fileType.SelectedIndex = 0;
        }

        private void isForbidSomeButtons(bool isforbid = true)
        {
            isContainerSons.IsEnabled = !isforbid;
            isContainerHideFiles.IsEnabled = !isforbid;
            fileType.Visibility = isforbid ? Visibility.Hidden : Visibility.Visible;
        }

        //去除文件中的隐藏文件
        private void removeHideFiles()
        {
            for(int i = 0; i < fileLists.Count; i++)
            {
                if((new FileInfo(fileLists[i]).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    fileLists.RemoveAt(i);
                    i--;
                }
            }
        }

        private void refreshPreviewLabel()
        {
            if(IsLoaded)
            {
                if(fileLists != null && fileLists.Count > 0)
                {
                    currentFileNameLabel.Content = System.IO.Path.GetFileName(fileLists[0]).Replace("_", "__");
                    string temp = renameFile(fileLists[0], 0, false);
                    if(temp != null)
                    {
                        newFileNameLabel.Content = System.IO.Path.GetFileName(temp).Replace("_", "__");

                    }
                }
                //ComboBoxItem cbb = selectMoble.SelectedItem as ComboBoxItem;
                //if(cbb != null)
                //{
                //    string str = cbb.Content as string;
                //    System.Diagnostics.Trace.WriteLine("cbb.Content --> " + str);
                //    if(str.IndexOf("(已修改)") < 0)
                //    {
                //        ComboBoxItem cbi = new ComboBoxItem();
                //        cbi.Content = str + "(已修改)";

                //        selectMoble.Items.Insert(selectMoble.Items.Count - 1, cbi);
                //        selectMoble.SelectedIndex = selectMoble.Items.Count - 2;
                //    }
                //}
            }
        }

        private void clearFileLists()
        {
            fileLists.Clear();
            renameFolderText.Text = "";
            renameFolderRoot.Clear();
            fileListBox.Items.Clear();
            currentFileNameLabel.Content = "";
            newFileNameLabel.Content = "";
            fileListTile.Content = "文件列表(共 0 个文件)";
        }
        #endregion

        #region 重命名
        private string renameFile(string path, int index, bool checkCopyToFolder)
        {
            string rtn = "", rtnFileName = "", rtnExtension = "";
            string tempStr;
            int tempNum;
            System.IO.FileInfo fileInfo = new FileInfo(path);
            fileInfo.Refresh();
            System.IO.DirectoryInfo di = fileInfo.Directory;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.FullName);
            string fileEx = fileInfo.Extension;
            rtnExtension = fileEx;
            int length = renameListsStackPanel.Children.Count;
            RenameSelectSingl temp;
            for(int i = 0; i < length; i++)
            {
                temp = renameListsStackPanel.Children[i] as RenameSelectSingl;
                if(temp != null)
                {
                    switch(temp.TypeUint)
                    {
                        case 0:
                            //文本
                            rtnFileName += temp.textSelected.Text;
                            break;
                        case 1:
                            //新扩展名
                            if((bool)temp.extension2Selected.IsChecked)
                            {
                                rtnExtension = temp.extension1Selected.Text;
                            }
                            else
                            {
                                rtnExtension = fileEx + temp.extension1Selected.Text;
                            }
                            break;
                        case 2:
                            //当前文件名
                            tempStr = "";
                            switch(temp.nowName1Selected.SelectedIndex)
                            {
                                case 0:
                                    tempStr = fileName + fileEx;
                                    break;
                                case 1:
                                    tempStr = fileName;
                                    break;
                                case 2:
                                    tempStr = fileEx;
                                    break;
                            }
                            switch(temp.nowName2Selected.SelectedIndex)
                            {
                                case 0:
                                    rtnFileName += tempStr;
                                    break;
                                case 1:
                                    rtnFileName += tempStr.ToUpper();
                                    break;
                                case 2:
                                    rtnFileName += tempStr.ToLower();
                                    break;
                            }
                            break;
                        case 3:
                            //序列数字
                            try
                            {
                                tempNum = Convert.ToInt32(temp.orderNum1Selected.Text) + index;
                                rtnFileName += tempNum.ToString("D" + (temp.orderNum2Selected.SelectedIndex + 1));
                            }
                            catch(Exception)
                            {
                                throw;
                            }
                            break;
                        case 4:
                            tempStr = "";
                            tempNum = index;
                            System.Diagnostics.Trace.WriteLine("index --> " + tempNum);
                            while(tempNum > -1)
                            {
                                tempStr = Encoding.ASCII.GetString(new byte[] { (byte)(97 + (tempNum % 26)) }) + tempStr;
                                System.Diagnostics.Trace.WriteLine("tempStr --> " + tempNum);
                                tempNum = tempNum / 26;
                                tempNum = tempNum - 1;
                            }
                            //序列字母
                            if(temp.orderLetterSelected.SelectedIndex == 1)
                            {
                                tempStr = tempStr.ToUpper();
                            }

                            rtnFileName += tempStr;
                            break;
                        case 5:
                            //时间日期
                            tempStr = (temp.time2Selected.SelectedItem as ComboBoxItem).Name.Replace("time_", "");
                            switch(temp.time1Selected.SelectedIndex)
                            {
                                case 0:
                                    //创建日期
                                    if(tempStr == "milis")
                                    {
                                        rtnFileName += fileInfo.CreationTime.Ticks;
                                    }
                                    else
                                    {
                                        rtnFileName += fileInfo.CreationTime.ToString(tempStr);
                                    }
                                    break;
                                case 1:
                                    //最后修改时间
                                    if(tempStr == "milis")
                                    {
                                        rtnFileName += fileInfo.LastWriteTime.Ticks;
                                    }
                                    else
                                    {
                                        rtnFileName += fileInfo.LastWriteTime.ToString(tempStr);
                                    }
                                    break;
                                case 2:
                                    //最后访问时间
                                    if(tempStr == "milis")
                                    {
                                        rtnFileName += fileInfo.LastAccessTime.Ticks;
                                    }
                                    else
                                    {
                                        rtnFileName += fileInfo.LastAccessTime.ToString(tempStr);
                                    }
                                    break;
                                case 3:
                                    //今天(相对当前机子时间)
                                    if(tempStr == "milis")
                                    {
                                        rtnFileName += DateTime.Now.Ticks;
                                    }
                                    else
                                    {
                                        rtnFileName += DateTime.Now.ToString(tempStr);
                                    }
                                    break;
                                case 4:
                                    //昨天(相对当前机子时间)
                                    if(tempStr == "milis")
                                    {
                                        rtnFileName += DateTime.Now.AddDays(-1).Ticks;
                                    }
                                    else
                                    {
                                        rtnFileName += DateTime.Now.AddDays(-1).ToString(tempStr);
                                    }
                                    break;
                            }
                            break;
                        case 6:
                            //文件夹名称
                            rtnFileName += di.Name;
                            break;
                        case 7:
                            //字符串替换
                            tempStr = "";
                            if((bool)temp.renameBefore.IsChecked)
                            {
                                tempStr = fileName + ((bool)temp.replaceStr3Selected.IsChecked ? fileEx : "");
                            }
                            else if((bool)temp.renameAfter.IsChecked)
                            {
                                tempStr = rtnFileName + ((bool)temp.replaceStr3Selected.IsChecked ? rtnExtension : "");
                            }
                            bool isIgnore = (bool)temp.isIgnoreCase.IsChecked;
                            bool isUseReg = (bool)temp.useRegular.IsChecked;
                            //System.Windows.MessageBox.Show(tempStr);
                            Regex tempReg;
                            if(temp.searchString.Text.Length > 0)
                            {
                                if(isIgnore)
                                {
                                    if(isUseReg)
                                    {
                                        try
                                        {
                                            tempReg = new Regex(temp.searchString.Text, RegexOptions.IgnoreCase);
                                            tempStr = tempReg.Replace(tempStr, temp.replaceString.Text);
                                        }
                                        catch
                                        {
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        tempStr = ignoreCaseReplace(tempStr, temp.searchString.Text, temp.replaceString.Text);
                                    }
                                }
                                else
                                {
                                    if(isUseReg)
                                    {
                                        try
                                        {
                                            tempReg = new Regex(temp.searchString.Text);
                                            tempStr = tempReg.Replace(tempStr, temp.replaceString.Text);
                                        }
                                        catch
                                        {
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        tempStr = tempStr.Replace(temp.searchString.Text, temp.replaceString.Text);
                                    }
                                }
                            }
                            if(temp.replaceStr4Selected.IsChecked == true)
                                rtnFileName = tempStr;
                            else
                                rtnFileName += tempStr;
                            if((bool)temp.replaceStr3Selected.IsChecked)
                            {
                                rtnExtension = System.IO.Path.GetExtension(di.FullName + @"\" + rtnFileName);
                                rtnFileName = System.IO.Path.GetFileNameWithoutExtension(di.FullName + @"\" + rtnFileName);
                            }
                            break;
                    }
                }
            }

            rtn = rtnFileName + rtnExtension;
            if(rtn.Length > 0 && rtn.Substring(rtn.Length - 1, 1) == @".")
                rtn += fileEx.Replace(@".", "");
            if(checkCopyToFolder && (toFoldItems.SelectedIndex == 1 || toFoldItems.SelectedIndex == 2) && Directory.Exists(goalFoldLabel.Content as string))
            {
                rtn = (goalFoldLabel.Content as string) + @"\" + rtn;
            }
            else
            {
                rtn = di.FullName + @"\" + rtn;
            }
            return rtn;
        }
        //忽略大小写, 进行替换文本
        private string ignoreCaseReplace(string old, string searchStr, string replaceStr)
        {
            string rtn = "";
            int length = old.Length;
            searchStr = searchStr.ToLower();
            int slens = searchStr.Length;
            for(int i = 0; i < length; i++)
            {
                if(i < length - slens + 1 && searchStr == old.Substring(i, slens).ToLower())
                {
                    rtn += replaceStr;
                    i += slens - 1;
                }
                else
                {
                    rtn += old.Substring(i, 1);
                }
            }
            return rtn;
        }

        private void RenameConfirm()
        {
            //System.Windows.MessageBox.Show("正在重命名, 请稍等....");
            modelManageBtns.Visibility = System.Windows.Visibility.Collapsed;
            InputModelName.Visibility = System.Windows.Visibility.Collapsed;
            popupWindowTxt.Visibility = System.Windows.Visibility.Visible;
            popupWindowTxt.Text = "正 在 重 命 名, 请　稍　等. . . .";
            DoubleAnimatonOpacity(0, 1, 0.5);
            if(renameFolderRoot.Count > 0)
            {
                string type = fileType.SelectedItem as string;
                if(type == "所有")
                    type = "";
                refreshFiles(type);
            }
            int length = fileLists.Count;
            List<string> afters = new List<string>();
            if(toFoldItems.SelectedIndex == 1 || toFoldItems.SelectedIndex == 2)
            {
                if(!Directory.Exists(goalFoldLabel.Content as string))
                {
                    if(System.Windows.MessageBox.Show("请选择正确的输出路径, 或者取消\"复制到文件夹\"选项") == MessageBoxResult.OK)
                        DoubleAnimatonOpacity(1, 0, 0.1);
                    return;
                }
            }
            for(int i = 0; i < length; i++)
            {
                afters.Add(renameFile(fileLists[i], i, true));
            }
            if(toFoldItems.SelectedIndex == 1)
            {
                for(int i = 0; i < length; i++)
                {
                    if(File.Exists(fileLists[i]))
                        File.Copy(fileLists[i], afters[i]);//resetAfterName(afters[i]));
                }
            }
            else
            {
                for(int i = 0; i < length; i++)
                {
                    if(File.Exists(fileLists[i]))
                        File.Move(fileLists[i], afters[i]);//resetAfterName(afters[i]));
                }
            }
            if(renameFolderRoot.Count > 0)
            {
                refreshFiles("");
                refreshFileType();
            }
            else
            {
                fileLists = afters;
                addToFileListBox(fileListBox, fileLists);
            }
            popupWindowTxt.Text = "重 命 名 完 成...";
            DoubleAnimatonOpacity(1, 0, 1);
        }
        private void DoubleAnimatonOpacity(double from, double to, double time)
        {
            if(to == 1)
                popupWindow.Visibility = System.Windows.Visibility.Visible;
            DoubleAnimation da = new DoubleAnimation();
            da.Duration = new Duration(TimeSpan.FromSeconds(time));
            da.From = from;
            da.To = to;
            Storyboard.SetTarget(da, popupWindow);
            Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
            Storyboard sb = new Storyboard();
            sb.Children.Add(da);
            sb.Completed += delegate(object sender, EventArgs args)
            {
                if(to == 0)
                    popupWindow.Visibility = System.Windows.Visibility.Collapsed;
            };

            sb.Begin();
        }

        //如果有重名对象, 加"_数字",重新命名
        //private string resetAfterName(string afterName)
        //{
        //    string rtn = afterName;
        //    if(File.Exists(afterName))
        //    {
        //        string fileName = System.IO.Path.GetFileNameWithoutExtension(afterName);
        //        string extension = System.IO.Path.GetExtension(afterName);
        //        string folderName = System.IO.Path.GetDirectoryName(afterName);
        //        int i = 0;
        //        while(File.Exists(rtn))
        //        {
        //            rtn = folderName + @"\" + fileName + "_" + i + extension;
        //            i++;
        //        }
        //    }
        //    return rtn;
        //}
        private void renamePreview()
        {
            //重命名预览
            if(fileLists.Count > 0)
            {
                if(renameFolderRoot.Count > 0)
                {
                    string type = fileType.SelectedItem as string;
                    if(type == "所有")
                        type = "";
                    refreshFiles(type);
                }
                int length = fileLists.Count;
                List<string> afters = new List<string>();
                if(toFoldItems.SelectedIndex == 1 || toFoldItems.SelectedIndex == 2)
                {
                    if(!Directory.Exists(goalFoldLabel.Content as string))
                    {
                        System.Windows.MessageBox.Show("请选择正确的输出路径, 或者选择\"原文件夹\"选项");
                        return;
                    }
                }
                for(int i = 0; i < length; i++)
                {
                    if(File.Exists(fileLists[i]))
                        afters.Add(renameFile(fileLists[i], i, true));
                }
                if(previewWindow == null)
                {
                    previewWindow = new PreviewWindow();
                    //previewWindow.Owner = this;
                    previewWindow.onRefreshPreview += renamePreview;
                }
                previewWindow.openWindow(fileLists, afters);
                previewWindow.Show();
            }
        }
        #endregion

        #region 事件监听函数
        //窗口加载完成后
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int length = RenameModels.RenameModelSingle.Models.Count;
            for(int i = 0; i < length; i++)
            {
                addToModelComboBox(RenameModels.RenameModelSingle.Models[i]);
            }
            selectMoble.SelectedIndex = 1;
            (renameListsStackPanel.Children[0] as RenameSelectSingl).onRefreshRequest += onRenameSelectSingleRequestHandle;
            (renameListsStackPanel.Children[0] as RenameSelectSingl).TypeUint = 2;
            renameFolderRoot = new List<string>();
            string[] temp = RenameModels.RenameModelSingle.LastRootFolder.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < temp.Length; i++)
            {
                temp[i] = temp[i].Trim();
                if(Directory.Exists(temp[i]))
                    renameFolderRoot.Add(temp[i]);
            }
            if(renameFolderRoot.Count > 0)
            {
                renameFolderText.Text = string.Join("; ", renameFolderRoot);
                refreshFiles("");
                refreshFileType();
            }
        }
        //浏览按钮---选择要重命名的文件夹
        private void renameFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            browserFolder.ShowNewFolderButton = false;
            System.Windows.Forms.DialogResult result = browserFolder.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                //copyToFold.IsChecked = false; 
                renameFolderRoot.Clear();
                renameFolderRoot.Add(browserFolder.SelectedPath);
                renameFolderText.Text = renameFolderRoot[0];
                refreshFiles("");
                refreshFileType();
            }
        }
        //
        //private void resetFolderBtn_Click(object sender = null, RoutedEventArgs e = null)
        //{
        //    fileListBox.Items.Clear();
        //    fileLists.Clear();
        //    renameFolderRoot.Clear();
        //    renameFolderText.Text = "";
        //    renameListsStackPanel.Children.RemoveRange(1, renameListsStackPanel.Children.Count - 1);
        //    (renameListsStackPanel.Children[0] as RenameSelectSingl).allItems.SelectedIndex = 0;
        //    (renameListsStackPanel.Children[0] as RenameSelectSingl).textSelected.Text = "";
        //    (renameListsStackPanel.Children[0] as RenameSelectSingl).deleteBtn.IsEnabled = false;
        //    (renameListsStackPanel.Children[0] as RenameSelectSingl).downBtn.IsEnabled = false;
        //    (renameListsStackPanel.Children[0] as RenameSelectSingl).upBtn.IsEnabled = false;
        //    currentFileNameLabel.Content = "";
        //    newFileNameLabel.Content = "";
        //}
        //更改文件类型
        private void fileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string type = fileType.SelectedItem as string;
            if(type == "所有")
                type = "";
            refreshFiles(type);
        }
        //是否包括子文件夹
        private void isContainerSons_Checked(object sender, RoutedEventArgs e)
        {
            if(renameFolderRoot.Count > 0)
            {
                //copyToFold.IsChecked = false;
                refreshFiles("");
                refreshFileType();
            }
        }

        //在显示要命名文件夹的文本框, 输入回车时, 调用
        private void renameFolderText_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Return || e.Key == Key.Enter)
            {
                if(Directory.Exists(renameFolderText.Text))
                {
                    renameFolderRoot.Clear();
                    renameFolderRoot.Add(renameFolderText.Text);
                    //copyToFold.IsChecked = false;
                    refreshFiles("");
                    refreshFileType();
                }
            }
        }

        //显示要命名文件夹的文本框失去焦点时调用
        private void renameFolderText_LostFocus(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(renameFolderText.Text))
            {
                renameFolderRoot.Clear();
                renameFolderRoot.Add(renameFolderText.Text);
                //copyToFold.IsChecked = false;
                refreshFiles("");
                refreshFileType();
            }
            renameFolderText.PreviewMouseDown += new MouseButtonEventHandler(renameFolderText_PreviewMouseDown);
        }

        //显示要命名文件夹的文本框获得焦点时调用
        private void renameFolderText_GotFocus(object sender, RoutedEventArgs e)
        {
            if(sender != null)
            {
                renameFolderText.SelectAll();
                renameFolderText.PreviewMouseDown -= new MouseButtonEventHandler(renameFolderText_PreviewMouseDown);
            }
        }
        //鼠标按下前调用
        private void renameFolderText_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(sender != null)
            {
                renameFolderText.Focus();
                e.Handled = true;
            }
        }

        //是否需要移动或复制到其他文件夹
        private void toFoldItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.IsLoaded)
            {
                if(toFoldItems.SelectedIndex == 1 || toFoldItems.SelectedIndex == 2)
                {
                    goalLineLabel.Visibility = System.Windows.Visibility.Visible;
                    goalFoldLabel.Visibility = System.Windows.Visibility.Visible;
                    goalBrowserBtn.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    goalLineLabel.Visibility = System.Windows.Visibility.Collapsed;
                    goalFoldLabel.Visibility = System.Windows.Visibility.Collapsed;
                    goalBrowserBtn.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

        }
        //浏览按钮---选择要复制到的目标文件夹
        private void goalBrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            browserFolder.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = browserFolder.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                goalFoldLabel.Content = browserFolder.SelectedPath;
            }
        }

        private void onRenameSelectSingleRequestHandle(object sender, EventArgs e, RenameSelectSingl newRSS = null)
        {
            if(newRSS != null)
                newRSS.onRefreshRequest += onRenameSelectSingleRequestHandle;
            refreshPreviewLabel();
        }

        //点击 -- 预览
        private void previewBtn_Click(object sender, RoutedEventArgs e)
        {
            renamePreview();
        }

        private void RenameBtn_Click(object sender, RoutedEventArgs e)
        {
            RenameConfirm();
        }
        //将文件或者文件夹拖拽到窗口
        private void RenameMainWindow_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if(e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                Array temp = ((System.Array)e.Data.GetData(System.Windows.DataFormats.FileDrop));
                //
                if(temp != null)
                {
                    if(Directory.Exists(temp.GetValue(0).ToString()))
                    {
                        renameFolderRoot.Clear();
                        renameFolderText.Text = "";
                        StringBuilder sb = new StringBuilder();
                        for(int i = 0; i < temp.Length; i++)
                        {
                            if(Directory.Exists(temp.GetValue(i).ToString()))
                            {
                                renameFolderRoot.Add(temp.GetValue(i).ToString());
                                sb.Append(renameFolderRoot[i]);
                                if(i < temp.Length - 1)
                                    sb.Append("; ");
                            }
                        }
                        renameFolderText.Text = sb.ToString();
                        refreshFiles("");
                        refreshFileType();
                    }
                    if(File.Exists(temp.GetValue(0).ToString()))
                    {
                        renameFolderRoot.Clear();
                        renameFolderText.Text = "";
                        isForbidSomeButtons(true);
                        fileListBox.Items.Clear();
                        for(int i = 0; i < temp.Length; i++)
                        {
                            if(File.Exists(temp.GetValue(i).ToString()))
                            {
                                fileLists.Add(temp.GetValue(i).ToString());
                            }
                        }
                        addToFileListBox(fileListBox, fileLists);
                        refreshPreviewLabel();
                    }
                }
            }
        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(!e.IsRepeat && e.Key == Key.F5)
            {
                if(renameFolderRoot.Count > 0)
                {
                    refreshFiles("");
                    refreshFileType();
                }
            }
        }

        private void selectMoble_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(IsLoaded && (selectMoble.SelectedItem as ComboBoxItem) != null)
            {
                string cn = (selectMoble.SelectedItem as ComboBoxItem).Name;
                switch(cn)
                {
                    case "resetAll":
                        resetRenameListByModel(RenameModels.RenameModelSingle.ResetModel);
                        break;
                    case "default":
                        resetRenameListByModel(RenameModels.RenameModelSingle.DefaultModel);
                        break;
                    case "last":
                        resetRenameListByModel(RenameModels.RenameModelSingle.LastModel);
                        break;
                    default:
                        resetRenameListByModel(cn);
                        break;

                }
                //removeModifyItems();
            }
        }

        private void manageModels_Click(object sender, RoutedEventArgs e)
        {
            popupWindowTxt.Visibility = System.Windows.Visibility.Collapsed;
            InputModelName.Visibility = System.Windows.Visibility.Collapsed;
            modelManageBtns.Visibility = System.Windows.Visibility.Visible;
            manageModelList.Items.Clear();
            int length = RenameModels.RenameModelSingle.Models.Count;
            int selectIndex = -1;
            for(int i = 0; i < length; i++)
            {
                ListBoxItem li = new ListBoxItem();
                li.Name = RenameModels.RenameModelSingle.Models[i].ID;
                li.Content = RenameModels.RenameModelSingle.Models[i].Name;
                if(li.Name == (selectMoble.SelectedItem as ComboBoxItem).Name)
                {
                    selectIndex = i;
                }
                manageModelList.Items.Add(li);
            }
            manageModelList.SelectedIndex = selectIndex;
            DoubleAnimatonOpacity(0, 1, 0.5);
        }
        //点击"管理模板"中的按钮
        private void modelManageBtns_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button current = e.OriginalSource as System.Windows.Controls.Button;
            ListBoxItem lbi = manageModelList.SelectedItem as ListBoxItem;
            string modelID = null;
            if(lbi != null)
                modelID = lbi.Name;
            if(current != null)
            {
                switch(current.Name)
                {
                    case "defaultCurrentModel":
                        //设置为默认模板
                        if(lbi != null)
                        {
                            RenameModels.RenameModelSingle.DefaultModel = RenameModels.RenameModelSingle.GetModel(modelID);
                            System.Windows.MessageBox.Show("已成功将 " + lbi.Name + " 设置为默认模版");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("请选择要设置默认的模版");
                        }
                        break;
                    case "delelteCurrentModel":
                        //删除当前选择的模板
                        if(lbi != null)
                        {
                            RenameModels.RenameModelSingle.RemoveModel(modelID);
                            manageModelList.Items.Remove(lbi);
                            removeFromModelComboBox(modelID);
                            System.Windows.MessageBox.Show("已成功将 " + lbi.Name + " 删除");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("请选择要删除的模版");
                        }
                        break;
                    case "refreshCurrentModel":
                        //更新当前选择的模板
                        if(lbi != null)
                        {
                            RenameModels.RenameModelSingle.refreshModel(convertToModel(RenameModels.RenameModelSingle.GetModel(modelID)));
                            System.Windows.MessageBox.Show("已成功更新模版 " + lbi.Name);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("请选择要更新的模版");
                        }
                        break;
                    case "saveCurrentModel":
                        //保存当前使用的模板
                        modelManageBtns.Visibility = System.Windows.Visibility.Collapsed;
                        InputModelName.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "cancel":
                        //完成
                        DoubleAnimatonOpacity(1, 0, 0.5);
                        break;
                }
            }

        }

        //点击输入模板名称中的按钮
        private void InputModelName_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button current = e.OriginalSource as System.Windows.Controls.Button;
            if(current != null)
            {
                switch(current.Content as string)
                {
                    case "确 定":
                        Model md = convertToModel(ModelNameTextBox.Text);
                        RenameModels.RenameModelSingle.AddModel(md);
                        addToModelComboBox(md);
                        DoubleAnimatonOpacity(1, 0, 0.5);
                        break;
                    case "取 消":
                        InputModelName.Visibility = System.Windows.Visibility.Collapsed;
                        modelManageBtns.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }
        }
        private void clearFileList_Click(object sender, RoutedEventArgs e)
        {
            clearFileLists();
        }

        //程序关闭时, 保存当前使用的模板
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            convertToModel(RenameModels.RenameModelSingle.LastModel);
            RenameModels.RenameModelSingle.LastModel.ToXMLElement();
            if(RenameModels.RenameModelSingle.LastModel.XE.ParentNode == null)
                RenameModels.RenameModelSingle.XD.DocumentElement.AppendChild(RenameModels.RenameModelSingle.LastModel.XE);
            //if(renameFolderText.Text.Length > 0)
            RenameModels.RenameModelSingle.LastRootFolder = renameFolderText.Text;
            RenameModels.RenameModelSingle.saveXML();
        }
        #endregion

    }
}
