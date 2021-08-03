using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Discount.UI
{
	public class TeamSelectButton : Button
	{
		protected Team team_;
		protected Color notHoveredColor_;
		protected Color hoveredColor_;

		protected Label playerCount_;

		public TeamSelectButton(Team team)
		{
			team_ = team;

			notHoveredColor_ = Teams.GetDarkTeamColor( team );

			notHoveredColor_.a = 0.9f;

			string buttonLabelText;

			switch ( team )
			{
				case Team.Spectator:
					buttonLabelText = "Spectate";
					break;

				case Team.Red:
					buttonLabelText = "Join Red Team";
					break;

				case Team.Blue:
					buttonLabelText = "Join Blue Team";
					break;

				default:
					buttonLabelText = "Join Random Team";
					notHoveredColor_ = new Color( 0.4f, 0.2f, 0.4f, 0.9f );
					break;
			}

			hoveredColor_ = new Color(
				notHoveredColor_.r + 0.1f,
				notHoveredColor_.g + 0.1f,
				notHoveredColor_.b + 0.1f,
				0.9f );

			playerCount_ = Add.Label( "", "player-count" );
			Add.Label( buttonLabelText, "label" );
		}

		public override void Tick()
		{
			base.Tick();

			if (HasHovered)
			{
				Style.BackgroundColor = hoveredColor_;
			}
			else
			{
				Style.BackgroundColor = notHoveredColor_;
			}

			if ( team_ > 0 )
			{
				playerCount_.Text = Teams.GetTeamPlayerCount( team_ ) + " players";
			}
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			string teamNameString;

			switch (team_)
			{
				case Team.Spectator:
					teamNameString = "spectator";
					break;

				case Team.Red:
					teamNameString = "red";
					break;

				case Team.Blue:
					teamNameString = "blue";
					break;

				default:
					teamNameString = "auto";
					break;
			}

			DiscountGame.JoinTeamCommand( teamNameString );
		}
	}
}
