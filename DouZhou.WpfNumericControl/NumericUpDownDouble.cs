using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using static System.Net.Mime.MediaTypeNames;

namespace DouZhou.WpfNumericControl
{
    /// <summary>
    /// double类型的数字控件
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class NumericUpDownDouble : NumericUpDownBase, INumericUpDown<double>
    {
        static NumericUpDownDouble()
        {
            //重写默认样式
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDownDouble), new FrameworkPropertyMetadata(typeof(NumericUpDownDouble)));
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
            //减少按钮执行
            var value = Value - Increment;
            if (value < Minimum)
            {
                //小于最小值时，设置为最小值
                Value = Minimum;
            }
            else
            {
                Value = (double)value;
            }
        }

        protected override void IncButton_Execute(object obj)
        {
            //增加按钮执行
            var value = Value + Increment;
            if (value > Maximum)
            {
                //大于最大值时，设置为最大值
                Value = Maximum;
            }
            else
            {
                Value = (double)value;
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
                Value = 0.0f;
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
            else if (CharUnicodeInfo.GetUnicodeCategory(sValueString[0]) == UnicodeCategory.CurrencySymbol)
            {
                //首字符为货币符号时，去掉货币符号
                sValueString = sValueString.Substring(1);
            }
            else if (sValueString.Length > 1 && CharUnicodeInfo.GetUnicodeCategory(sValueString[1]) == UnicodeCategory.CurrencySymbol)
            {
                //负数且有货币符号时，去掉货币符号
                sValueString = sValueString.Remove(1,1);
            }
            else if(sValueString.EndsWith("%", StringComparison.OrdinalIgnoreCase))
            {
                //百分比格式时，去掉百分号
                sValueString = sValueString.Substring(0, sValueString.Length - 1);
                if (double.TryParse(sValueString,out double percentValue))
                {
                    //百分比显示的数值是实际值的100倍
                    percentValue /= 100.0;
                }
                sValueString = percentValue.ToString();
            }
            //尝试转换成decimal类型
            if (double.TryParse(sValueString, style, culture, out double value))
            {
                //转换成功后，强制转换成double类型
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
                    Value = (double)value;
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
                var newValue = (-Value);    //减号键会将值取反
                if (newValue < Minimum)
                {
                    newValue = Minimum;
                }
                if (newValue > Maximum)
                {
                    newValue = Maximum;
                }
                Value = (double)newValue;
                e.Handled = true;
            }
            if(e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                //小数点键
                var curText = ((TextBox) sender).Text;
                if (curText.Contains("."))
                {
                    //已经有小数点了
                    e.Handled = true;
                }
                return;
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
        /// 格式化字符串，支持：C、E、G、R,P0-P9,F0-F9,N0-N9
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        /// <summary>
        /// 格式化字符串，支持：C、E、G、R,P0-P9,F0-F9,N0-N9
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat", typeof(string), typeof(NumericUpDownDouble), new PropertyMetadata("G", OnStringFormatChanged, OnFloatStringFormatCoerceValue));



        private static void OnStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //当StringFormat属性改变时，更新ValueString属性
            if (d is NumericUpDownDouble numeric)
            {
                numeric.ValueString = UpdateValueString((string)e.NewValue, numeric.Value);
            }
        }
        #endregion
        #region 依赖属性方式实现接口属性
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumericUpDownDouble), new PropertyMetadata((double)0, OnValueChanged, OnCoerceValue));

        private static object OnCoerceValue(DependencyObject d, object baseValue)
        {
            //强制值在最大值和最小值之间
            if (d is NumericUpDownDouble numeric)
            {
                double value = (double)baseValue;
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
            return double.MinValue;
        }

        /// <summary>
        /// 值变更时调用
        /// </summary>
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericUpDownDouble numeric)
            {
                //获取旧值
                double oldValue = (double)e.OldValue;
                //获取新值
                double newValue = (double)e.NewValue;
                IsValueChanged = true;
                numeric.ValueString = UpdateValueString(numeric.StringFormat, newValue);
                IsValueChanged = false;
                //引发ValueChanged事件
                numeric.OnValueChanged(oldValue, newValue);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private static string UpdateValueString(string stringFormat, double newValue)
        {
            try
            {
                return newValue.ToString(stringFormat);
            }
            catch (Exception)
            {
                return newValue.ToString();
            }
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(NumericUpDownDouble), new PropertyMetadata(double.MinValue, OnMinimumChanged, OnMinimumCoerceValue));

        private static object OnMinimumCoerceValue(DependencyObject d, object baseValue)
        {
            //最小值不能大于最大值
            if (d is NumericUpDownDouble numeric)
            {
                double value = (double)baseValue;
                if (value > numeric.Maximum)
                {
                    return numeric.Maximum;
                }
                return value;
            }
            return double.MinValue;
        }

        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //当最小值改变时，强制值不小于最小值
            if (d is NumericUpDownDouble numeric)
            {
                if (numeric.Value < numeric.Minimum)
                {
                    numeric.Value = numeric.Minimum;
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }
        //以依赖属性的方式实现最大值
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(NumericUpDownDouble), new PropertyMetadata(double.MaxValue, OnMaximumChanged, OnMaximumCoerceValue));

        private static object OnMaximumCoerceValue(DependencyObject d, object baseValue)
        {
            //最大值不能小于最小值
            if (d is NumericUpDownDouble numeric)
            {
                double value = (double)baseValue;
                if (value < numeric.Minimum)
                {
                    return numeric.Minimum;
                }
                return value;
            }
            return double.MaxValue;
        }

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //当最大值改变时，强制值不大于最大值
            if (d is NumericUpDownDouble numeric)
            {
                if (numeric.Value > numeric.Maximum)
                {
                    numeric.Value = numeric.Maximum;
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }
        //以依赖属性的方式实现单击按钮一次增加或减少的值
        public double Increment
        {
            get { return (double)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }
        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register("Increment", typeof(double), typeof(NumericUpDownDouble), new PropertyMetadata((double)1, OnIncrementChanged, OnIncrementCoerceValue));

        private static object OnIncrementCoerceValue(DependencyObject d, object baseValue)
        {
            //增量不能小于1
            if (d is NumericUpDownDouble numeric)
            {
                double value = (double)baseValue;
                if (value <= 0)
                {
                    //增量不能小于1（整数类型）
                    return (double)1;
                }
                return value;
            }
            return (double)1;
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
                                                                                                typeof(RoutedPropertyChangedEventHandler<double>),
                                                                                                typeof(NumericUpDownDouble));
        /// <summary>
        /// 值改变事件
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        /// <summary>
        /// 引发ValueChanged事件
        /// </summary>
        /// <param name="oldValue">旧的值</param>
        /// <param name="newValue">新的值</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
            RoutedPropertyChangedEventArgs<double> arg = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent);
            RaiseEvent(arg);
        }
        #endregion

    }
}
