using System.Windows;
using System.Windows.Controls;

namespace RenameApp
{
    public class ItemEvenOddStyleSelector:StyleSelector
    {
        #region 属性
        /// <summary>
        /// 偶数行样式
        /// </summary>
        public Style EvenStyle
        {
            get;
            set;
        }
        /// <summary>
        /// 奇数行样式
        /// </summary>
        public Style OddStyle
        {
            get;
            set;
        }
        #endregion

        #region 方法
        public override Style SelectStyle(object item, DependencyObject container)
        {
            ListBox listBox = ItemsControl.ItemsControlFromItemContainer(container) as ListBox;
            int index = listBox.ItemContainerGenerator.IndexFromContainer(container);
            if(index % 2 == 0)
            {
                return EvenStyle;
            }
            else
            {
                return OddStyle;
            }
        }
        #endregion
    }
}
