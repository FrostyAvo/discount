using Sandbox;

namespace Discount
{
	public abstract partial class TeamBuilding : ModelEntity, ITeamEntity
	{
		[Net]
		public Team Team { get; set; }
	}
}
