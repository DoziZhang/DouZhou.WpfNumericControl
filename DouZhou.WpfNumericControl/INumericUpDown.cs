namespace DouZhou.WpfNumericControl
{
    /// <summary>
    /// 数值控件接口（请使用依赖属性的方式实现该接口属性，且其元数据有强制值回调和值变更通知）
    /// </summary>
    /// <typeparam name="T">数值类型</typeparam>
    public interface INumericUpDown<T> where T : struct
    {
        /// <summary>
        /// 值（请使用依赖属性的方式实现该属性）
        /// </summary>
        T Value { get; set; }
        /// <summary>
        /// 最小值（请使用依赖属性的方式实现该属性）
        /// </summary>
        T Minimum { get; set; }
        /// <summary>
        /// 最大值（请使用依赖属性的方式实现该属性）
        /// </summary>
        T Maximum { get; set; }
        /// <summary>
        /// 单击按钮一次增加或减少的值（请使用依赖属性的方式实现该属性）
        /// </summary>
        T Increment { get; set; }
    }
}
