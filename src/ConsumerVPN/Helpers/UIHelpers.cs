using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;

namespace WLVPN.Helpers
{
    public static class UIHelpers
    {
        #region Is In Design Mode
        private static bool? _isInDesignMode;

        public static bool IsInDesignMode(DependencyObject dependencyObject)
        {
            if (!_isInDesignMode.HasValue)
            {
                _isInDesignMode = DesignerProperties.GetIsInDesignMode(dependencyObject)
                    || (null == Application.Current)
                    || Application.Current.GetType() == typeof(Application);
            }

            return _isInDesignMode.Value;
        }
        #endregion

        #region Find Visual Child
        public static T FindVisualChild<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T item)
                {
                    return item;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Find Visual Child
        public static T FindVisualChild<T>(DependencyObject dependencyObject, DependencyObject childToFind) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T item && child == childToFind)
                {
                    return item;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child, childToFind);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Find Visual Child
        public static bool IsChild(DependencyObject dependencyObject, DependencyObject childToFind)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child != null && child == childToFind)
                {
                    return true;
                }
                else
                {
                    if (IsChild(child, childToFind))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Find Visual Parent
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        /// Call like Window owner = UIHelper.FindVisualParent"Window"(mycontrol);
        public static T FindVisualParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = GetParentObject(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {

                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }


        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            if (child is ContentElement contentElement)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                return contentElement is FrameworkContentElement fce ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            if (child is FrameworkElement frameworkElement)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        #endregion

        #region GetVisualChildCollection
        public static List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }
        private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }
        #endregion

        #region Run On Display Thread
        public static void RunOnDisplayThread(Action actionToExecute)
        {
            var dispatcher = Application.Current?.Dispatcher;

            if (dispatcher != null && dispatcher.CheckAccess() == false)
            {
                dispatcher.BeginInvoke(actionToExecute);
            }
            else
            {
                actionToExecute();
            }
        }
        #endregion

        #region DelayRun
        public static void DelayRun(Action actionToExecute, TimeSpan timeToWait)
        {
            Task.Run(() =>
            {
                Thread.Sleep(timeToWait);
                RunOnDisplayThread(actionToExecute);
            });
        }
        #endregion

        #region Run At
        /// <summary>
        /// RunAt. Run action at specific time.
        /// </summary>
        /// <param name="actionToExecute"></param>
        /// <param name="timeToRun"></param>
        /// <returns>
        /// 0 = action scheduled
        /// 1 = action ran
        /// -1 = action not run
        /// </returns>
        public static int RunAt(Action actionToExecute, DateTime timeToRun, bool runanyway)
        {
            if (DateTime.Now.CompareTo(timeToRun) < 0)
            {
                var diff = timeToRun.Subtract(DateTime.Now);
                DelayRun(actionToExecute, diff);
                return 0;
            }
            // the time of execution has passed. 
            if (runanyway) RunOnDisplayThread(actionToExecute);
            return runanyway ? 1 : -1;
        }
        #endregion
    }
}
