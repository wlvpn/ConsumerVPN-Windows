// <copyright file="LoadSorter.cs" company="WLVPN">
// Copyright (c) WLVPN. All Rights Reserved.
// </copyright>

using VpnSDK.Interfaces;
using System.ComponentModel;

namespace WLVPN.Behaviors
{
    /// <summary>
    /// Class LoadSorter. Provides a sorter to sort by the Load value on an <see cref="ILocation"/>.
    /// </summary>
    /// <seealso cref="ICustomSorter" />
    public class LoadSorter : ICustomSorter
    {
        /// <inheritdoc/>
        public ListSortDirection SortDirection { get; set; }

        /// <inheritdoc/>
        public int Compare(object x, object y)
        {
            // Make Best Available always on top of the list.
            if (y is IBestAvailable)
            {
                return 1;
            }

            if (x is IBestAvailable)
            {
                return -1;
            }

            // Proceed with the countries sorting
            int result;
            if (((IRegion)x)?.Load == null)
            {
                result = -1;
            }
            else if (((IRegion)y)?.Load == null)
            {
                result = 1;
            }
            else
            {
                if (((IRegion)x).Load > ((IRegion)y).Load)
                {
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }

            if (SortDirection == ListSortDirection.Descending)
            {
                return -result;
            }

            return result;
        }
    }
}