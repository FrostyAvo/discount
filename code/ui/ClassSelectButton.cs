using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Discount.UI
{
	public class ClassSelectButton : Button
	{
		protected string className_;
		protected Team clientTeam_;
		protected Color notHoveredColor_;
		protected Color hoveredColor_;

		public ClassSelectButton( string className )
		{
			className_ = className;

			// Invalid team to force an initial update
			clientTeam_ = (Team)(-1);

			UpdateTeam();

			string buttonLabelText = className;

			// Capitalize class name
			if ( className.Length != 0 )
			{
				buttonLabelText = className.Remove(0, 1).Insert(0, className[0].ToString().ToUpper() );
			}

			Add.Label( buttonLabelText, "label" );
		}

		public override void Tick()
		{
			base.Tick();

			UpdateTeam();

			if (HasHovered)
			{
				if ( Style.BackgroundColor != hoveredColor_ )
				{
					Style.BackgroundColor = hoveredColor_;
					Style.Dirty();
				}
			}
			else
			{
				if ( Style.BackgroundColor != notHoveredColor_ )
				{
					Style.BackgroundColor = notHoveredColor_;
					Style.Dirty();
				}
			}
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			DiscountGame.JoinClassCommand( className_ );
		}

		protected void UpdateTeam()
		{
			Team clientTeam = Team.Unassigned;

			if ( Game.Current is DiscountGame discountGame
				&& discountGame.Teams != null )
			{
				clientTeam = discountGame.Teams.GetClientTeam( Local.Client );
			}

			if ( clientTeam != clientTeam_ )
			{
				clientTeam_ = clientTeam;

				notHoveredColor_ = Teams.GetDarkTeamColor( clientTeam_ );
				notHoveredColor_.a = 0.9f;

				hoveredColor_ = new Color(
					notHoveredColor_.r + 0.1f,
					notHoveredColor_.g + 0.1f,
					notHoveredColor_.b + 0.1f,
					0.9f );

				Style.BackgroundColor = notHoveredColor_;
				Style.Dirty();

				SkipTransitions();
			}
		}
	}
}
