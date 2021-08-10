using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Discount.UI
{
	public class ControlPointIcon : Panel
	{
		protected Label statusLabel_;
		protected Panel progressBar_;
		protected ControlPoint controlPoint_;

		public ControlPointIcon( ControlPoint point, Panel parent )
		{
			controlPoint_ = point;
		    Parent = parent;

			progressBar_ = Add.Panel( "capture-progress-bar" );
			statusLabel_ = Add.Label( "", "status-label" );
		}

		public override void Tick()
		{
			base.Tick();

			Color iconColor = controlPoint_.Locked ?
				Teams.GetDarkTeamColor(controlPoint_.OwningTeam)
				: Teams.GetLightTeamColor( controlPoint_.OwningTeam );
			Color progressColor = Teams.GetLightTeamColor( controlPoint_.CapturingTeam );

			if ( Style.BackgroundColor != iconColor )
			{
				Style.BackgroundColor = iconColor;
				Style.Dirty();
			}

			if ( progressBar_.Style.BackgroundColor != progressColor )
			{
				progressBar_.Style.BackgroundColor = progressColor;
			}

			progressBar_.Style.Height = new Length()
			{
				Unit = LengthUnit.Percentage,
				Value = controlPoint_.CaptureProgress * 100
			};
			progressBar_.Style.Dirty();

			if ( controlPoint_.Locked )
			{
				statusLabel_.Text = "🔒";
				return;
			}

			if ( controlPoint_.Blocked )
			{
				statusLabel_.Text = "‍🚫";
				return;
			}

			int capturerCount = controlPoint_.CapturerCount();

			if ( capturerCount > 0 )
			{
				statusLabel_.Text = capturerCount + "🚶‍";
			}
			else
			{
				statusLabel_.Text = "";
			}
		}
	}
}
