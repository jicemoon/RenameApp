using System.Windows;
using System.Windows.Controls;

namespace RenameApp
{
    /// <summary>
    /// TextBoxPromat.xaml 的交互逻辑
    /// </summary>
    public partial class TextBoxPromat:TextBox
    {
        /// <summary>
        /// 提示字符串
        /// </summary>
        public static readonly DependencyProperty PromatStringProperty = DependencyProperty.Register("PromatString", typeof(string), typeof(TextBoxPromat), new PropertyMetadata(""));
        /// <summary>
        /// 提示字符串的透明度(0-1), 默认0.4
        /// </summary>
        public static readonly DependencyProperty PromatOpacityProperty = DependencyProperty.Register("PromatOpacity", typeof(double), typeof(TextBoxPromat), new PropertyMetadata(0.4));
        /// <summary>
        /// 提示字符串
        /// </summary>
        public string PromatString
        {
            get
            {
                return (string)GetValue(PromatStringProperty);
            }
            set
            {
                SetValue(PromatStringProperty, value);
            }
        }
        /// <summary>
        /// 提示字符串的透明度(0-1), 默认0.4
        /// </summary>
        public double PromatOpacity
        {
            get
            {
                return (double)GetValue(PromatOpacityProperty);
            }
            set
            {
                SetValue(PromatOpacityProperty, value);
            }
        }
        /// <summary>
        /// 带有提示字符的TextBox
        /// </summary>
        public TextBoxPromat()
        {
            InitializeComponent();
        }
    }
}
