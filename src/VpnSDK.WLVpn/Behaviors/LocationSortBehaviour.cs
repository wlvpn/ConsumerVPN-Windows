// <copyright file="LocationSortBehaviour.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VpnSDK.WLVpn.Behaviors
{
    internal class LocationSortBehaviour
    {
        public static readonly DependencyProperty CustomSorterProperty =
            DependencyProperty.RegisterAttached("CustomSorter", typeof(ICustomSorter), typeof(LocationSortBehaviour));

        public static readonly DependencyProperty AllowCustomSortProperty = DependencyProperty.RegisterAttached(
            "AllowCustomSort",
            typeof(bool),
            typeof(LocationSortBehaviour),
            new UIPropertyMetadata(false, OnAllowCustomSortChanged));

        public static ICustomSorter GetCustomSorter(DataGridColumn gridColumn)
        {
            return (ICustomSorter)gridColumn.GetValue(CustomSorterProperty);
        }

        public static void SetCustomSorter(DataGridColumn gridColumn, ICustomSorter value)
        {
            gridColumn.SetValue(CustomSorterProperty, value);
        }

        public static bool GetAllowCustomSort(DataGrid grid)
        {
            return (bool)grid.GetValue(AllowCustomSortProperty);
        }

        public static void SetAllowCustomSort(DataGrid grid, bool value)
        {
            grid.SetValue(AllowCustomSortProperty, value);
        }

        private static void OnAllowCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var existing = d as DataGrid;

            if (existing == null)
            {
                return;
            }

            if (!(bool)e.OldValue && (bool)e.NewValue)
            {
                existing.Sorting += HandleCustomSorting;
            }
            else
            {
                existing.Sorting -= HandleCustomSorting;
            }
        }

        private static void HandleCustomSorting(object sender, DataGridSortingEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid == null || !GetAllowCustomSort(dataGrid))
            {
                return;
            }

            ListCollectionView listColView = dataGrid.ItemsSource as ListCollectionView;
            if (listColView == null)
            {
                throw new InvalidCastException("ItemsSource property must be type of ListCollectionView.");
            }

            ICustomSorter sorter = GetCustomSorter(e.Column);
            if (sorter == null)
            {
                return;
            }

            e.Handled = true;

            ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending)
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            e.Column.SortDirection = direction;
            sorter.SortDirection = direction;
            listColView.CustomSort = sorter;
        }
    }
}
