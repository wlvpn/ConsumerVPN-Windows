// <copyright file="LocationSortBehaviour.cs" company="WLVPN">
// Copyright (c) WLVPN. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using WLVPN.Helpers;

namespace WLVPN.Behaviors
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

        public static ICustomSorter GetCustomSorter(DataGridColumn dataGridColumn)
        {
            return (ICustomSorter)dataGridColumn.GetValue(CustomSorterProperty);
        }

        public static void SetCustomSorter(DataGridColumn dataGridColumn, ICustomSorter value)
        {
            dataGridColumn.SetValue(CustomSorterProperty, value);
        }

        public static bool GetAllowCustomSort(DataGrid dataGrid)
        {
            return (bool)dataGrid.GetValue(AllowCustomSortProperty);
        }

        public static void SetAllowCustomSort(DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(AllowCustomSortProperty, value);
        }

        private static void OnAllowCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DataGrid;

            if (dataGrid == null || UIHelpers.IsInDesignMode(d))
            {
                return;
            }
            if (!(bool)e.OldValue && (bool)e.NewValue)
            {
                dataGrid.Sorting += HandleCustomSorting;
                dataGrid.Loaded += Existing_Loaded;
            }
            else
            {
                dataGrid.Sorting -= HandleCustomSorting;
                dataGrid.Loaded -= Existing_Loaded;
            }
        }

        private static void Existing_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid == null)
            {
                return;
            }

            ListCollectionView collection = dataGrid.Items.SourceCollection as ListCollectionView;

            if (collection?.CustomSort == null && dataGrid.Columns.Count > 1)
            {
                List<DataGridColumn> columns = dataGrid.Columns.Where((x) => (bool)x.GetValue(Selector.IsSelectedProperty)).ToList();
                if (columns.Count > 0)
                {
                    HandleCustomSorting(dataGrid, new DataGridSortingEventArgs(columns[0]));
                }
                else
                {
                    HandleCustomSorting(dataGrid, new DataGridSortingEventArgs(dataGrid.Columns[1]));
                }
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
