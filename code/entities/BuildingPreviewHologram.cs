using Sandbox;

namespace Discount
{
	public class BuildingPreviewHologram : ModelEntity
	{
		public override void Spawn()
		{
			EnableSolidCollisions = false;
			EnableShadowCasting = false;
		}
	}
}
