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

			TeamSelectButton redSelectButton = new TeamSelectButton( Team.Red );
			TeamSelectButton blueSelectButton = new TeamSelectButton( Team.Blue );
			TeamSelectButton randomButton = new TeamSelectButton( (Team)(-1) );
			TeamSelectButton spectateSelectButton = new TeamSelectButton( Team.Spectator );

			buttonPanel.AddChild( redSelectButton );
			buttonPanel.AddChild( blueSelectButton );
			buttonPanel.AddChild( randomButton );
			buttonPanel.AddChild( spectateSelectButton );
		}
	}
}

