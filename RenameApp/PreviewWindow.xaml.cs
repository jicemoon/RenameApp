using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RenameApp
{
    /// <summary>
    /// PreviewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreviewWindow:Window
    {
        #region 字段
        private int currentSeleted = -1;
        public delegate void RefreshPreview();
        public event RefreshPreview onRefreshPreview;
        #endregion

        #region 构造方法
        public PreviewWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region 公共方法
        public void openWindow(List<string> befores, List<string> afters)
        {
            orderNumList.Children.Clear();
            renameBeforeList.Children.Clear();
            renameAfterList.Children.Clear();
            int length = befores.Count;
            for(int i = 0; i < length; i++)
            {
                orderNumList.Children.Add(proLabel((i + 1).ToString(), i, HorizontalAlignment.Right));
                renameBeforeList.Children.Add(proLabel(befores[i], i, HorizontalAlignment.Left));
                renameAfterList.Children.Add(proLabel(afters[i], i, HorizontalAlignment.Left));
            }
        }
        #endregion

        #region 私有方法
        private Label proLabel(string str, int index, HorizontalAlignment ha)
        {
            Label label = new Label();
            if(index % 2 == 0)
                label.Style = FindResource("oddStyle") as Style;
            else
                label.Style = FindResource("evenStyle") as Style;
            label.HorizontalContentAlignment = ha;
            label.Content = str.Replace("_", "__");
            label.MouseDown += label_MouseDown;
            label.MouseEnter += label_MouseEnter;
            return label;
        }
        private void refreshPreviewList()
        {
            if(onRefreshPreview != null)
                onRefreshPreview();
        }
        #endregion

        #region 事件监听
        void label_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Label lb = sender as Label;
            StackPanel par = lb.Parent as StackPanel;
            int index = par.Children.IndexOf(lb);
            resetCurrentLable(renameBeforeList, "currentOverStyle", index);
            resetCurrentLable(orderNumList, "currentOverStyle", index);
            resetCurrentLable(renameAfterList, "currentOverStyle", index);
        }

        void label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Label lb = sender as Label;
            StackPanel par = lb.Parent as StackPanel;
            int index = par.Children.IndexOf(lb);
            currentSeleted = index;
            resetCurrentLable(renameBeforeList, "currentSelectedStyle");
            resetCurrentLable(orderNumList, "currentSelectedStyle");
            resetCurrentLable(renameAfterList, "currentSelectedStyle");
        }
        private void resetCurrentLable(StackPanel par, string style, int current = -1)
        {
            int length = par.Children.Count;
            for(int i = 0; i < length; i++)
            {
                Label label = par.Children[i] as Label;
                if(i == current)
                {
                    label.Style = FindResource(style) as Style;
                    continue;
                }
                if(i == currentSeleted)
                {
                    label.Style = FindResource("currentSelectedStyle") as Style;
                    continue;
                }
                if(i % 2 == 0)
                    label.Style = FindResource("oddStyle") as Style;
                else
                    label.Style = FindResource("evenStyle") as Style;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            if(sv != null && e.VerticalChange != 0)
            {
                leftSV.ScrollToVerticalOffset(sv.VerticalOffset);
                rightSV.ScrollToVerticalOffset(sv.VerticalOffset);
                orderSV.ScrollToVerticalOffset(sv.VerticalOffset);
            }
        }
        #endregion

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            refreshPreviewList();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(!e.IsRepeat)
            {
                if(e.Key == System.Windows.Input.Key.F5)
                {
                    refreshPreviewList();
                }
            }
        }

    }
}
