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
	}
}

