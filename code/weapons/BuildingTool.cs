using Sandbox;
using System.Collections.Generic;

namespace Discount.Weapons
{
	public partial class BuildingTool<T> : Weapon where T : TeamBuilding, new()
	{
		[Net]
		protected BuildingPreviewHologram PreviewHologram { get; set; }

		public override string ViewModelPath => "weapons/rust_shotgun/v_rust_shotgun.vmdl";

		protected string BuildingModel = "";
		protected float BuildingModelScale = 1f;

		public BuildingTool()
		{

		}

		public override void Spawn()
		{
			base.Spawn();

			// This is a hacky solution but works fine
			T dummyBuilding = new T();

			BuildingModel = dummyBuilding.Model;
			BuildingModelScale = dummyBuilding.ModelScale;

			dummyBuilding.Delete();

			SetModel( "weapons/rust_shotgun/rust_shotgun.vmdl" );

			if ( PreviewHologram == null )
			{
				PreviewHologram = new BuildingPreviewHologram();
				PreviewHologram.Scale = BuildingModelScale;
				PreviewHologram.Owner = this;

				PreviewHologram.SetModel( BuildingModel );

				PreviewHologram.EnableDrawing = false;
			}
		}

		public override void Simulate( Client player )
		{
			base.Simulate( player );

			if ( Owner is TeamPlayer teamPlayer
				&& teamPlayer.OwnsBuildingOfType<T>() )
			{
				PreviewHologram.EnableDrawing = false;

				return;
			}

			Vector3 traceStart = Owner.EyePos + Rotation.FromYaw( Owner.EyeRot.Yaw() ).Forward * 60f;

			IEnumerator<TraceResult> traceResults = TraceBullet(
				traceStart,
				traceStart + Vector3.Down * 80f
				).GetEnumerator();

			// Only grab the first trace result and reject traces that didn't hit
			if ( !traceResults.MoveNext() || !traceResults.Current.Hit )
			{
				PreviewHologram.EnableDrawing = false;

				return;
			}

			TraceResult traceResult = traceResults.Current;

			if ( !traceResult.Entity.IsWorld
				|| Vector3.GetAngle( traceResult.Normal, Vector3.Up ) > 45f )
			{
				PreviewHologram.RenderColor = new Color( 1f, 0, 0, 0.6f );
			}
			else
			{
				PreviewHologram.RenderColor = new Color( 0, 1f, 0, 0.6f );
			}

			PreviewHologram.Position = traceResult.EndPos;
			PreviewHologram.Rotation = Rotation.FromYaw( Rotation.Yaw() + 90f );

			PreviewHologram.EnableDrawing = true;
		}

		public override void AttackPrimary()
		{
			// No point doing any of this on the client since the server is going to create the building anyway
			if ( !IsServer )
			{
				return;
			}

			Vector3 traceStart = Owner.EyePos + Rotation.FromYaw( Owner.EyeRot.Yaw() ).Forward * 60f;

			IEnumerator<TraceResult> traceResults = TraceBullet(
				traceStart,
				traceStart + Vector3.Down * 80f
				).GetEnumerator();

			// Only grab the first trace result and reject traces that didn't hit
			if ( !traceResults.MoveNext() || !traceResults.Current.Hit )
			{
				return;
			}

			TraceResult traceResult = traceResults.Current;

			// Only create buildings on the ground
			if ( !traceResult.Entity.IsWorld
				|| Vector3.GetAngle( traceResult.Normal, Vector3.Up ) > 45f )
			{
				return;
			}

			TeamPlayer teamPlayer = Owner as TeamPlayer;

			// Don't allow more than one building per type
			if ( teamPlayer != null
				&& teamPlayer.OwnsBuildingOfType<T>() )
			{
				return;
			}

			T createdBuilding = new T();

			createdBuilding.Team = teamPlayer != null ? teamPlayer.Team : Team.Unassigned;
			createdBuilding.Position = traceResult.EndPos;
			createdBuilding.Owner = Owner;
			createdBuilding.Rotation = Rotation.FromYaw( Rotation.Yaw() + 90f );

			createdBuilding.Spawn();
		}

		public override void AttackSecondary()
		{
			if ( IsServer )
			{
				(Owner as TeamPlayer)?.TryGetOwnedBuildingOfType<T>()?.Delete();
			}
		}

		public override string ToString()
		{
			return "Building Tool";
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 3 );
			anim.SetParam( "aimat_weight", 0.2f );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			if ( IsServer )
			{
				PreviewHologram?.Delete();
			}
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			base.ActiveEnd( ent, dropped );

			if ( PreviewHologram != null )
			{
				PreviewHologram.EnableDrawing = false;
			}
		}
	}
}
