// <copyright file="ICustomSorter.cs" company="WLVPN">
// Copyright (c) WLVPN. All Rights Reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLVPN.Behaviors
{
    /// <summary>
    /// Interface ICustomSorter. Describes a custom sorting behavior for WPF.
    /// </summary>
    /// <seealso cref="System.Collections.IComparer" />
    public interface ICustomSorter : IComparer
    {
        /// <summary>
        /// Gets or sets the sort direction.
        /// </summary>
        /// <value>The sort direction.</value>
        ListSortDirection SortDirection { get; set; }
    }
}