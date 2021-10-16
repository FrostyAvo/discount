using Sandbox;

namespace Discount
{
	public abstract partial class TeamBuilding : ModelEntity, ITeamEntity
	{
		[Net]
		public Team Team { get; set; }

		public abstract string Model { get; }
		public abstract float ModelScale { get; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( Model );

			Scale = ModelScale;

			MoveType = MoveType.Physics;
			CollisionGroup = CollisionGroup.Interactive;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			Health = 150;

			RenderColor = Teams.GetLightTeamColor( Team );
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", Position );
			PlaySound( "building.break" );

			if ( IsServer )
			{
				Delete();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			(Owner as TeamPlayer)?.RemoveOwnedBuilding( this );
		}
	}
}
