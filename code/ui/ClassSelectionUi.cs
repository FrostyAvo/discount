using Sandbox;
using Sandbox.UI;

namespace Discount.UI
{
	[Library]
	public class ClassSelectionUi : HudEntity<RootPanel>
	{
		public ClassSelectionUi()
		{
			if ( !IsClient )
			{
				return;
			}

			RootPanel.StyleSheet.Load( "/ui/ClassSelectionUi.scss" );

			Panel buttonPanel = RootPanel.AddChild<Panel>("buttons");

			Label label = RootPanel.AddChild<Label>( "select-class-label" );

			label.Text = "Select a Class";

			foreach (string className in ClassData.AllClasses)
			{
				buttonPanel.AddChild( new ClassSelectButton( className ) );
			}

			buttonPanel.AddChild( new ClassSelectButton( "random" ) );
		}

		public void Enable()
		{
			RootPanel.Style.Display = DisplayMode.Flex;
			RootPanel.Style.PointerEvents = "all";
			RootPanel.Style.Dirty();
		}

		public void Disable()
		{
			RootPanel.Style.Display = DisplayMode.None;
			RootPanel.Style.PointerEvents = "none";
			RootPanel.Style.Dirty();
		}
	}
}

