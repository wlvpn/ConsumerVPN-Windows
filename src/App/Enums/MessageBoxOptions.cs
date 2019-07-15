using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLVPN.Enums
{
    [Flags]
    public enum MessageBoxOptions
    {
        Ok = 2,
        Cancel = 4,
        Yes = 8,
        No = 16,

        OkCancel = Ok | Cancel,
        YesNo = Yes | No,
        YesNoCancel = Yes | No | Cancel
    }
}
