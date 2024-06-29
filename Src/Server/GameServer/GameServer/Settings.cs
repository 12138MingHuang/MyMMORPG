using System;

namespace GameServer.Properties
{

    /// <summary>
    /// 部分定义 Settings 类，用于处理设置类的特定事件。
    /// </summary>
    /// <remarks>
    /// 可以处理以下事件：
    /// - 在更改某个设置的值之前将引发 SettingChanging 事件。
    /// - 在更改某个设置的值之后将引发 PropertyChanged 事件。
    /// - 在加载设置值之后将引发 SettingsLoaded 事件。
    /// - 在保存设置值之前将引发 SettingsSaving 事件。
    /// </remarks>
    public sealed partial class Settings
    {

        public Settings()
        {
            // 若要为保存和更改设置添加事件处理程序，请取消注释下列行: 

            this.SettingChanging += this.SettingChangingEventHandler;

            this.SettingsSaving += this.SettingsSavingEventHandler;

        }

        /// <summary>
        /// 设置更改事件处理程序。
        /// </summary>
        /// <param name="sender">事件的发送者。</param>
        /// <param name="e">包含事件数据的 SettingChangingEventArgs 对象。</param>
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            string settingName = e.SettingName;
            object newValue = e.NewValue;

            // 根据需要处理设置更改事件
            Console.WriteLine($"设置 '{settingName}' 正在更改为 '{newValue}'.");

            // 如果需要比较旧值，可以在此之前将旧值保存下来
            object oldValue = e.SettingName;
            Console.WriteLine($"设置 '{settingName}' 的旧值为 '{oldValue}'.");

            // 示例：如果某些设置值不符合要求，可以取消设置更改
            if (newValue.ToString() == "invalid_value")
            {
                e.Cancel = true;
                Console.WriteLine("由于值无效，取消设置更改。");
            }
        }

        /// <summary>
        /// 设置保存事件处理程序。
        /// </summary>
        /// <param name="sender">事件的发送者。</param>
        /// <param name="e">包含事件数据的 CancelEventArgs 对象。</param>
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 示例：在保存设置之前执行的操作
            Console.WriteLine("正在保存设置...");

            // 示例：根据条件决定是否取消保存操作
            bool cancelSave = false; // 根据实际条件设置

            if (cancelSave)
            {
                e.Cancel = true;
                Console.WriteLine("取消了保存设置操作。");
            }
            else
            {
                Console.WriteLine("设置保存成功。");
            }
        }
    }
}
