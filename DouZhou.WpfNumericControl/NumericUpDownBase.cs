using Microsoft.SqlServer.Server;

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DouZhou.WpfNumericControl
{
    /// <summary>
    /// 在TextBox控件失去焦点/Enter键按下时，尝试将文本框的值转换为Value
    /// </summary>
    [TemplatePart(Name = ElementTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = ElementIncreaseButton, Type = typeof(Button))]
    [TemplatePart(Name = ElementDecreaseButton, Type = typeof(Button))]
    public abstract class NumericUpDownBase : Control
    {

        protected const string ElementTextBox = "PART_TextBox";
        protected const string ElementIncreaseButton = "PART_IncreaseButton";
        protected const string ElementDecreaseButton = "PART_DecreaseButton";

        /// <summary>
        /// 值变更标识（在子类中使用，避免值变更更新文本时再次设置值而导致格式化显示值无法更新值）
        /// </summary>
        protected static bool IsValueChanged = false;

        /// <summary>
        /// 浮点数使用的字符串格式
        /// </summary>
        protected static object OnFloatStringFormatCoerceValue(DependencyObject d, object baseValue)
        {
            if (baseValue == null || !(baseValue is string sFormat) || string.IsNullOrWhiteSpace(sFormat))
            {
                //默认格式
                return "G";
            }
            else if (baseValue is string format && 
                (string.Equals("C",format,System.StringComparison.OrdinalIgnoreCase) 
                || string.Equals("E", format, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals("G", format, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals("R", format, System.StringComparison.OrdinalIgnoreCase)))
            {
                return format;  //浮点数支持的格式
            }
            else if (baseValue is string format2)
            {
                //以F/N/P开头的格式（不区分大小写）
                string pattern = @"^[FfPpNn]\d*$";
                if (System.Text.RegularExpressions.Regex.IsMatch(format2, pattern))
                {
                    return format2;
                }
                else
                    return "G";
            }
            return "G";
        }
        /// <summary>
        /// 整数使用的字符串格式
        /// </summary>
        protected static object OnStringFormatCoerceValue(DependencyObject d, object baseValue)
        {
            //只允许整数数据类型的格式
            if (baseValue == null || !(baseValue is string sFormat) || string.IsNullOrWhiteSpace(sFormat))
            {
                //默认格式
                return "G";
            }
            else if (baseValue is string format &&
                (string.Equals("C", format, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals("D", format, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals("E", format, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals("G", format, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals("X", format, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals("NO", format, System.StringComparison.OrdinalIgnoreCase)))
            {
                return format;  //整数支持的格式
            }
            return "G";
        }

        static NumericUpDownBase()
        {
            //重写默认样式
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDownBase), new FrameworkPropertyMetadata(typeof(NumericUpDownBase)));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(NumericUpDownBase), new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(NumericUpDownBase), new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }
        #region 要求子类重写的功能
        /// <summary>
        /// 要求子类重写，用于实现对Value的值的强制转换
        /// </summary>
        /// <returns></returns>
        protected abstract object CoereValueString(DependencyObject d, object baseValue);
        /// <summary>
        /// 要求子类重写，用于实现对ValueString的值变更时的逻辑
        /// </summary>
        protected abstract void ValueStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e);
        /// <summary>
        /// 判断值减少按键是否可用
        /// </summary>
        protected abstract bool DecButtonCanExecute(object obj);
        /// <summary>
        /// 执行值减少操作
        /// </summary>
        protected abstract void DecButton_Execute(object obj);

        /// <summary>
        /// 要求子类重写，以响应下调按钮的点击事件（判断是否可用）
        /// </summary>
        protected abstract bool InsButtonCanExecute(object obj);

        /// <summary>
        /// 要求子类重写，以响应上调按钮的点击事件
        /// </summary>
        /// <param name="obj"></param>
        protected abstract void IncButton_Execute(object obj);

        /// <summary>
        /// 要求子类重写，在文本框失去焦点时，尝试将文本框的值转换为Value
        /// </summary>
        protected abstract void TextBox_LostFocus(object sender, RoutedEventArgs e);

        /// <summary>
        /// 要求子类重写，以响应键盘上下键对值的加减操作
        /// </summary>
        protected abstract void TextBox_PreviewKeyDown(object sender, KeyEventArgs e);

        /// <summary>
        /// 要求子类重写，用于实现控件文本变更时的逻辑
        /// </summary>
        protected abstract void TextBox_TextChanged(object sender, TextChangedEventArgs e);
        #endregion
        #region 控件元素
        private TextBox _textBox;
        private Button _increaseButton;
        private Button _decreaseButton;

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (TextBox != null && !IsReadOnly && IsEnabled)
            {
                if (e.Delta > 0)
                {
                    IncButton_Execute(this);
                }
                else
                {
                    DecButton_Execute(this);
                }
            }
        }
        /// <summary>
        /// 元素TextBox
        /// </summary>
        protected TextBox TextBox
        {
            get { return _textBox; }
            set
            {
                if (_textBox != null)
                {
                    _textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
                    _textBox.TextChanged -= TextBox_TextChanged;
                    _textBox.LostFocus -= TextBox_LostFocus;
                }
                _textBox = value;
                if (_textBox != null)
                {
                    _textBox.TextChanged += TextBox_TextChanged;
                    _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                    _textBox.LostFocus += TextBox_LostFocus;
                    _textBox.SetBinding(SelectionBrushProperty, new Binding(SelectionBrushProperty.Name) { Source = this });
                    _textBox.SetBinding(SelectionOpacityProperty, new Binding(SelectionOpacityProperty.Name) { Source = this });
                    _textBox.SetBinding(CaretBrushProperty, new Binding(CaretBrushProperty.Name) { Source = this });
#if NET48_OR_GREATER
                    _textBox.SetBinding(SelectionTextBrushProperty, new Binding(SelectionTextBrushProperty.Name) { Source = this });
#endif
                    //TextBox是否只读
                    _textBox.SetBinding(TextBoxBase.IsReadOnlyProperty, new Binding(IsReadOnlyProperty.Name) { Source = this });
                }
            }
        }
        protected Button IncButton
        {
            get { return _increaseButton; }
            set
            {
                if (_increaseButton != null)
                {
                    _increaseButton.Command = null;
                }
                _increaseButton = value;
                if (_increaseButton != null)
                {
                    //Button是否可用=IsReadOnly的反值
                    _increaseButton.SetBinding(ButtonBase.IsEnabledProperty, new Binding(IsReadOnlyProperty.Name) { Source = this, Converter = new BooleanReverseConverter() });
                    _increaseButton.Command = new RelayCommand(IncButton_Execute, InsButtonCanExecute);
                }
            }
        }
        protected Button DecButton
        {
            get { return _decreaseButton; }
            set
            {
                if (_decreaseButton != null)
                {
                    _decreaseButton.Command = null;
                }
                _decreaseButton = value;
                if (_decreaseButton != null)
                {
                    //Button是否可用=IsReadOnly的反值
                    _decreaseButton.SetBinding(ButtonBase.IsEnabledProperty, new Binding(IsReadOnlyProperty.Name) { Source = this, Converter = new BooleanReverseConverter() });
                    _decreaseButton.Command = new RelayCommand(DecButton_Execute, DecButtonCanExecute);
                }
            }
        }
        /// <summary>
        /// 应用模板时调用
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TextBox = GetTemplateChild(ElementTextBox) as TextBox;
            IncButton = GetTemplateChild(ElementIncreaseButton) as Button;
            DecButton = GetTemplateChild(ElementDecreaseButton) as Button;
        }

        #endregion


        #region 依赖属性ValueString
        /// <summary>
        /// 显示的字符串
        /// </summary>
        public string ValueString
        {
            get { return (string)GetValue(ValueStringProperty); }
            set { SetValue(ValueStringProperty, value); }
        }
        public static readonly DependencyProperty ValueStringProperty =
            DependencyProperty.Register("ValueString", typeof(string), typeof(NumericUpDownBase), new PropertyMetadata("0", OnValueStringChanged, OnValueStringCoereValueCallBack));

        private static object OnValueStringCoereValueCallBack(DependencyObject d, object baseValue)
        {
            if (d is NumericUpDownBase numericBase)
            {
                return numericBase.CoereValueString(d, baseValue);
            }
            return baseValue;
        }
        private static void OnValueStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericUpDownBase numericBase)
            {
                numericBase.ValueStringChanged(d, e);
            }
        }
        #endregion
        #region 颜色控制
        // <summary>
        ///     是否显示上下调值按钮
        /// </summary>
        public static readonly DependencyProperty ShowUpDownButtonProperty = DependencyProperty.Register(
            nameof(ShowUpDownButton), typeof(bool), typeof(NumericUpDownBase), new PropertyMetadata(true));

        /// <summary>
        ///     是否显示上下调值按钮
        /// </summary>
        public bool ShowUpDownButton
        {
            get => (bool)GetValue(ShowUpDownButtonProperty);
            set => SetValue(ShowUpDownButtonProperty, value);
        }

        /// <summary>
        ///     标识 IsReadOnly 依赖属性。
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly), typeof(bool), typeof(NumericUpDownBase), new PropertyMetadata(false));

        /// <summary>
        ///     获取或设置一个值，该值指示NumericUpDown是否只读。
        /// </summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty SelectionBrushProperty =
        TextBoxBase.SelectionBrushProperty.AddOwner(typeof(NumericUpDownBase));

        public Brush SelectionBrush
        {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }
#if NET48_OR_GREATER

    public static readonly DependencyProperty SelectionTextBrushProperty =
        TextBoxBase.SelectionTextBrushProperty.AddOwner(typeof(NumericUpDownBase));

    public Brush SelectionTextBrush
    {
        get => (Brush) GetValue(SelectionTextBrushProperty);
        set => SetValue(SelectionTextBrushProperty, value);
    }

#endif

        public static readonly DependencyProperty SelectionOpacityProperty =
            TextBoxBase.SelectionOpacityProperty.AddOwner(typeof(NumericUpDownBase));

        public double SelectionOpacity
        {
            get => (double)GetValue(SelectionOpacityProperty);
            set => SetValue(SelectionOpacityProperty, value);
        }

        public static readonly DependencyProperty CaretBrushProperty =
            TextBoxBase.CaretBrushProperty.AddOwner(typeof(NumericUpDownBase));

        public Brush CaretBrush
        {
            get => (Brush)GetValue(CaretBrushProperty);
            set => SetValue(CaretBrushProperty, value);
        }
        #endregion
    }
}
