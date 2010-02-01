//copied from original
#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

#endregion

namespace Piannaf.Ports.Microsoft.Samples.WPF.Calculator
{
    class MyTextBox : System.Windows.Controls.TextBox
    {
        protected override void OnPreviewGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;
            base.OnPreviewGotKeyboardFocus(e);
        }


    }
}
