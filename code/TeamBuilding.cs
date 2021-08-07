using Sandbox;

namespace Discount
{
	public abstract partial class TeamBuilding : ModelEntity, ITeamEntity
	{
		[Net, Predicted] public Team Team { get; set; }
	}
}
