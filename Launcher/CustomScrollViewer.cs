using System.Windows.Controls;
using System.Windows.Input;

namespace Launcher
{
	internal class CustomScrollViewer : ScrollViewer
	{
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			e.Handled = false;
		}
	}
}
