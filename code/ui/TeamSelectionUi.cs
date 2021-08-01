using Sandbox;
using Sandbox.UI;

namespace Discount.UI
{
	[Library]
	public class TeamSelectionUi : HudEntity<RootPanel>
	{
		protected Button _selectRedButton;
		protected Button _selectBlueButton;
		protected Button _spectateButton;

		public TeamSelectionUi()
		{
			if ( !IsClient )
			{
				return;
			}

			RootPanel.StyleSheet.Load( "/ui/TeamSelectionUi.scss" );

			Panel buttonPanel = RootPanel.AddChild<Panel>("buttons");

			TeamSelectButton redSelectButton = new TeamSelectButton( 2 );
			TeamSelectButton blueSelectButton = new TeamSelectButton( 3 );
			TeamSelectButton randomButton = new TeamSelectButton( -1 );
			TeamSelectButton spectateSelectButton = new TeamSelectButton( 1 );

			buttonPanel.AddChild( redSelectButton );
			buttonPanel.AddChild( blueSelectButton );
			buttonPanel.AddChild( randomButton );
			buttonPanel.AddChild( spectateSelectButton );
		}
	}
}

