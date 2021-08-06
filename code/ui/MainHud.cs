using Sandbox;
using Sandbox.UI;

namespace Discount.UI
{
	[Library]
	public class MainHud : HudEntity<RootPanel>
	{
		public MainHud()
		{
			if ( !IsClient )
			{
				return;
			}

			RootPanel.StyleSheet.Load( "/ui/MainHud.scss" );

			RootPanel.AddChild<TeamNameTags>();
			RootPanel.AddChild<CrosshairCanvas>();
			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<KillFeed>();
			RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
			RootPanel.AddChild<HealthPanel>();
			RootPanel.AddChild<AmmoPanel>();
			RootPanel.AddChild<InventoryBar>();
		}
	}

}

