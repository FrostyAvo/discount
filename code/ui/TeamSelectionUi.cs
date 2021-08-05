using Sandbox;
using Sandbox.UI;

namespace Discount.UI
{
	[Library]
	public class TeamSelectionUi : HudEntity<RootPanel>
	{
		public TeamSelectionUi()
		{
			if ( !IsClient )
			{
				return;
			}

			RootPanel.StyleSheet.Load( "/ui/TeamSelectionUi.scss" );

			Panel buttonPanel = RootPanel.AddChild<Panel>("buttons");

			Label label = RootPanel.AddChild<Label>( "select-team-label" );

			label.Text = "Select a Team";

			buttonPanel.AddChild( new TeamSelectButton( Team.Red ) );
			buttonPanel.AddChild( new TeamSelectButton( Team.Blue ) );
			buttonPanel.AddChild( new TeamSelectButton( (Team)(-1) ) );
			buttonPanel.AddChild( new TeamSelectButton( Team.Spectator ) );
		}
	}
}

