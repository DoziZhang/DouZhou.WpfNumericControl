using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DouZhou.WpfNumericControl
{
    /// <summary>
    /// int类型的数字控件
    /// </summary>
    /// <remarks>
    /// 已知问题：当StringFormat属性为X或x时，只能输入一位数值，无法输入多位数值；只能通过增加按钮或减少按钮/上下键/粘贴来改变值
    /// </remarks>
    public class NumericUpDownInt32 : NumericUpDownBase, INumericUpDown<int>
    {
        static NumericUpDownInt32()
        {
            //重写默认样式
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDownInt32), new FrameworkPropertyMetadata(typeof(NumericUpDownInt32)));
        }
        #region 基类功能实现
        protected override void ValueStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //不要在此处设置Value属性，否则会导致在StringFormat为X或x等特殊格式化字符串时，只能输入一位数值，无法输入多位数值
        }
        protected override object CoereValueString(DependencyObject d, object baseValue)
        {
            //强制值文本
            return baseValue;
        }

        protected override bool DecButtonCanExecute(object obj)
        {
            //减少按钮是否可用
            return Value > Minimum;
        }

        protected override void DecButton_Execute(object obj)
        {
            if (!IsEnabled || IsReadOnly)
            {
                return;
            }
            //减少按钮执行(鼠标滚轮操作使会溢出）
            var value = Value < int.MinValue + Increment ? int.MinValue : Value - Increment;
            if (value < Minimum)
            {
                //小于最小值时，设置为最小值
                Value = Minimum;
            }
            else
            {
                Value = (int)value;
            }
        }

        protected override void IncButton_Execute(object obj)
        {
            if (!IsEnabled || IsReadOnly)
            {
                return;
            }
            //增加按钮执行(鼠标滚轮操作使会溢出）
            var value = Value > int.MaxValue - Increment ? int.MaxValue : Value + Increment;
            if (value > Maximum)
            {
                //大于最大值时，设置为最大值
                Value = Maximum;
            }
            else
            {
                Value = (int)value;
            }
        }

        protected override bool InsButtonCanExecute(object obj)
        {
            //增加按钮是否可用
            return Value < Maximum;
        }

        protected override void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //文本框失去焦点时，尝试更新值
            if (IsValueChanged == false)//避免重复设置值
                TryParseTextToValue();
        }

        private void TryParseTextToValue()
        {
            if (string.IsNullOrWhiteSpace(ValueString))
            {
                //文本框为空时，设置当前值文本
                Value = 0;
                ValueString = UpdateValueString(StringFormat, Value);
                return;
            }
            //由于textBox显示的字符串可能是格式化的字符串，所以需要尝试转换
            NumberStyles style = NumberStyles.Number | NumberStyles.Currency;
            if (string.Equals(StringFormat, "X", StringComparison.OrdinalIgnoreCase))
            {
                style = NumberStyles.HexNumber;
            }
            else if (string.Equals(StringFormat, "E", StringComparison.OrdinalIgnoreCase))
            {
                //科学计数法
                style = NumberStyles.Float | NumberStyles.AllowExponent;
            }
            CultureInfo culture = CultureInfo.CurrentCulture;
            string sValueString = ValueString;
            if (sValueString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {//十六进制格式时，去掉0x前缀,否则会导致只变更0x后的数值时，无法正确转换
                sValueString = sValueString.Substring(2);
            }
            else
            {
                //首字符为货币符号时，去掉货币符号
                if (CharUnicodeInfo.GetUnicodeCategory(sValueString[0]) == UnicodeCategory.CurrencySymbol)
                {
                    sValueString = sValueString.Substring(1);
                }
            }
            //尝试转换成decimal类型
            if (int.TryParse(sValueString, style, culture, out int value))
            {
                //转换成功后，强制转换成int类型
                if (value < Minimum)
                {
                    Value = Minimum;
                }
                else if (value > Maximum)
                {
                    Value = Maximum;
                }
                else
                {
                    Value = (int)value;
                }
                ValueString = UpdateValueString(StringFormat, Value);
            }
            else
            {
                //转换失败时，设置当前值文本
                ValueString = UpdateValueString(StringFormat, Value);
            }
        }

        protected override void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 检查是否按下了Ctrl+C或Ctrl+V
            if ((e.Key == Key.V || e.Key == Key.C) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // 允许Ctrl+V操作，不阻止事件继续传递
                return;
            }
            // 检查是否按下了Ctrl+A, 如果是则全选文本
            if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                TextBox.SelectAll();
                e.Handled = true;
                return;
            }
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                //数字键
                return;
            }
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right)
            {
                //允许删除键、回车键、左右键（光标移动）
                return;
            }
            if (e.Key == Key.Enter)
            {
                //回车键
                ValueString = ((TextBox)sender).Text;    //回车键会尝试更新值
                TryParseTextToValue();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Up)
            {
                IncButton_Execute(this);    //上键会触发增加操作
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Down)
            {
                DecButton_Execute(this);    //下键会触发减少操作
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                var newValue = -Value;
                if (newValue < Minimum)
                {
                    newValue = Minimum;
                }
                else if (newValue > Maximum)
                {
                    newValue = Maximum;
                }
                Value = newValue;
                e.Handled = true;
            }
            if (string.Equals(StringFormat, "X", StringComparison.OrdinalIgnoreCase) || string.Equals(StringFormat, "x", StringComparison.OrdinalIgnoreCase))
            {
                //十六进制格式时，允许输入A~F
                if (e.Key >= Key.A && e.Key <= Key.F)
                {
                    return;
                }
            }
            e.Handled = true;
        }

        protected override void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).Select((sender as TextBox).Text.Length, 0);
        }


        #endregion
        #region 依赖属性
        /// <summary>
        /// 格式化字符串，支持：G、N、X、x（通用、数字、十六进制、十六进制小写）
        /// </summary>
        /// <remarks>
        /// int类型支持的格式化字符串有：G、N0、X、x,D（通用、无小数数字、十六进制、十六进制小写、十进制数）
        /// </remarks>
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        /// <summary>
        /// 格式化字符串，支持：G、N、X、x（通用、数字、十六进制、十六进制小写）
        /// </summary>
        /// <remarks>
        /// int类型支持的格式化字符串有：G、N0、X、x,D（通用、无小数数字、十六进制、十六进制小写、十进制数）
        /// </remarks>
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat", typeof(string), typeof(NumericUpDownInt32), new PropertyMetadata("G", OnStringFormatChanged, OnStringFormatCoerceValue));



        private static void OnStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //当StringFormat属性改变时，更新ValueString属性
            if (d is NumericUpDownInt32 numeric)
            {
                numeric.ValueString = UpdateValueString((string)e.NewValue, numeric.Value);
            }
        }
        #endregion
        #region 依赖属性方式实现接口属性
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDownInt32), new PropertyMetadata((int)0, OnValueChanged, OnCoerceValue));

        private static object OnCoerceValue(DependencyObject d, object baseValue)
        {
            //强制值在最大值和最小值之间
            if (d is NumericUpDownInt32 numeric)
            {
                int value = (int)baseValue;
                if (value < numeric.Minimum)
                {
                    return numeric.Minimum;
                }
                else if (value > numeric.Maximum)
                {
                    return numeric.Maximum;
                }
                return value;
            }
            return int.MinValue;
        }

        /// <summary>
        /// 值变更时调用
        /// </summary>
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericUpDownInt32 numeric)
            {
                //获取旧值
                int oldValue = (int)e.OldValue;
                //获取新值
                int newValue = (int)e.NewValue;
                IsValueChanged = true;
                numeric.ValueString = UpdateValueString(numeric.StringFormat, newValue);
                IsValueChanged = false;
                //引发ValueChanged事件
                numeric.OnValueChanged(oldValue, newValue);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private static string UpdateValueString(string stringFormat, int newValue)
        {
            try
            {
                //十六进制、数字、通用格式
                if (string.Equals(stringFormat, "X", StringComparison.OrdinalIgnoreCase))
                {
                    string sFormat = "0x{0:" + stringFormat + "8}";
                    return string.Format(sFormat, newValue);
                }
                return newValue.ToString(stringFormat);
            }
            catch (Exception)
            {
                return newValue.ToString();
            }
        }

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumericUpDownInt32), new PropertyMetadata(int.MinValue, OnMinimumChanged, OnMinimumCoerceValue));

        private static object OnMinimumCoerceValue(DependencyObject d, object baseValue)
        {
            //最小值不能大于最大值
            if (d is NumericUpDownInt32 numeric)
            {
                int value = (int)baseValue;
                if (value > numeric.Maximum)
                {
                    return numeric.Maximum;
                }
                return value;
            }
            return int.MinValue;
        }

        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //当最小值改变时，强制值不小于最小值
            if (d is NumericUpDownInt32 numeric)
            {
                if (numeric.Value < numeric.Minimum)
                {
                    numeric.Value = numeric.Minimum;
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }
        //以依赖属性的方式实现最大值
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumericUpDownInt32), new PropertyMetadata(int.MaxValue, OnMaximumChanged, OnMaximumCoerceValue));

        private static object OnMaximumCoerceValue(DependencyObject d, object baseValue)
        {
            //最大值不能小于最小值
            if (d is NumericUpDownInt32 numeric)
            {
                int value = (int)baseValue;
                if (value < numeric.Minimum)
                {
                    return numeric.Minimum;
                }
                return value;
            }
            return int.MaxValue;
        }

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //当最大值改变时，强制值不大于最大值
            if (d is NumericUpDownInt32 numeric)
            {
                if (numeric.Value > numeric.Maximum)
                {
                    numeric.Value = numeric.Maximum;
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }
        //以依赖属性的方式实现单击按钮一次增加或减少的值
        public int Increment
        {
            get { return (int)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }
        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register("Increment", typeof(int), typeof(NumericUpDownInt32), new PropertyMetadata((int)1, OnIncrementChanged, OnIncrementCoerceValue));

        private static object OnIncrementCoerceValue(DependencyObject d, object baseValue)
        {
            //增量不能≤0
            if (d is NumericUpDownInt32 numeric)
            {
                int value = (int)baseValue;
                if (value <= 0)
                {
                    //增量不能≤0（整数类型）
                    return (int)1;
                }
                return value;
            }
            return (int)1;
        }

        private static void OnIncrementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //无需处理
        }
        #endregion
        #region 路由事件

        /// <summary>
        /// ValueChanged路由事件
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
                                                                                                RoutingStrategy.Bubble,
                                                                                                typeof(RoutedPropertyChangedEventHandler<int>),
                                                                                                typeof(NumericUpDownInt32));
        /// <summary>
        /// 值改变事件
        /// </summary>
        public event RoutedPropertyChangedEventHandler<int> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        /// <summary>
        /// 引发ValueChanged事件
        /// </summary>
        /// <param name="oldValue">旧的值</param>
        /// <param name="newValue">新的值</param>
        protected virtual void OnValueChanged(int oldValue, int newValue)
        {
            RoutedPropertyChangedEventArgs<int> arg = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue, ValueChangedEvent);
            RaiseEvent(arg);
        }
        #endregion

    }
}
