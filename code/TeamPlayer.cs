using Sandbox;

namespace Discount
{
	public abstract partial class TeamPlayer : Player
	{
		[Net, Predicted] public Team Team { get; set; }
	}
}
