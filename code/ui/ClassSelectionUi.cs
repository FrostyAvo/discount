using Sandbox;
using Sandbox.UI;

namespace Discount.UI
{
	[Library]
	public class ClassSelectionUi : HudEntity<RootPanel>
	{
		protected Panel buttonPanel_;

		public ClassSelectionUi()
		{
			if ( !IsClient )
			{
				return;
			}

			RootPanel.StyleSheet.Load( "/ui/ClassSelectionUi.scss" );

			buttonPanel_ = RootPanel.AddChild<Panel>("buttons");

			Label label = RootPanel.AddChild<Label>( "select-class-label" );

			label.Text = "Select a Class";

			foreach (string className in ClassData.AllClasses)
			{
				buttonPanel_.AddChild( new ClassSelectButton( className ) );
			}

			buttonPanel_.AddChild( new ClassSelectButton( "random" ) );
		}

		public void Enable()
		{
			RootPanel.Style.Display = DisplayMode.Flex;
			RootPanel.Style.PointerEvents = "all";
			RootPanel.Style.Dirty();

			// Update button colors
			foreach ( Panel child in buttonPanel_.Children )
			{
				child.Tick();
			}
		}

		public void Disable()
		{
			RootPanel.Style.Display = DisplayMode.None;
			RootPanel.Style.PointerEvents = "none";
			RootPanel.Style.Dirty();
		}
	}
}

