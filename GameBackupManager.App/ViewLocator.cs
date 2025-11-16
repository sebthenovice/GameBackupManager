using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GameBackupManager.App.ViewModels;

namespace GameBackupManager.App
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        #region Public Methods

        public Control? Build(object? data)
        {
            var name = data?.GetType().FullName?.Replace("ViewModel", "View");
            if (name == null)
                return new TextBlock { Text = "Invalid Data Type" };

            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "View Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }

        #endregion Public Methods
    }

    public abstract class ViewModelBase
    { }
}