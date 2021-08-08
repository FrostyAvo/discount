using Sandbox;
using System.Collections.Generic;

namespace Discount.Weapons
{
	public partial class DispenserBuilder : Weapon
	{
		[Net]
		protected BuildingPreviewHologram PreviewHologram { get; set; }

		public override string ViewModelPath => "weapons/rust_shotgun/v_rust_shotgun.vmdl";

		public DispenserBuilder()
		{
			
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_shotgun/rust_shotgun.vmdl" );

			if ( PreviewHologram == null )
			{
				PreviewHologram = new BuildingPreviewHologram();
				PreviewHologram.Scale = 0.8f;

				PreviewHologram.SetModel( "models/rust_props/electrical_boxes/electrical_box_b.vmdl" );

				PreviewHologram.EnableDrawing = false;
			}
			else
			{
				PreviewHologram.EnableDrawing = false;
			}
		}

		public override void Simulate( Client player )
		{
			base.Simulate( player );

			if ( Owner is TeamPlayer teamPlayer
				&& teamPlayer.OwnsBuildingOfType<Dispenser>() )
			{
				PreviewHologram.EnableDrawing = false;

				return;
			}

			if ( IsServer )
			{
				IEnumerator<TraceResult> traceResults = TraceBullet(
					Owner.EyePos,
					Owner.EyePos + Owner.EyeRot.Forward * 200f
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
					PreviewHologram.RenderColorAndAlpha = new Color32(255, 0, 0, 150);
				}
				else
				{
					PreviewHologram.RenderColorAndAlpha = new Color32( 0, 255, 0, 150 );
				}

				PreviewHologram.Position = traceResult.EndPos;
				PreviewHologram.Rotation = Rotation.FromYaw( Rotation.Yaw() + 90f );

				PreviewHologram.EnableDrawing = true;
			}
		}

		public override void AttackPrimary()
		{
			// No point doing any of this on the client since the server needs to create the dispenser anyway
			if ( !IsServer )
			{
				return;
			}

			IEnumerator<TraceResult> traceResults = TraceBullet(
				Owner.EyePos,
				Owner.EyePos + Owner.EyeRot.Forward * 200f
				).GetEnumerator();

			// Only grab the first trace result and reject traces that didn't hit
			if ( !traceResults.MoveNext() || !traceResults.Current.Hit )
			{
				return;
			}

			TraceResult traceResult = traceResults.Current;

			// Only create dispensers on the ground
			if ( !traceResult.Entity.IsWorld
				|| Vector3.GetAngle( traceResult.Normal, Vector3.Up ) > 45f )
			{
				return;
			}

			TeamPlayer teamPlayer = Owner as TeamPlayer;

			// Don't allow more than one dispenser
			if ( teamPlayer != null
				&& teamPlayer.OwnsBuildingOfType<Dispenser>() )
			{
				return;
			}

			new Dispenser()
			{
				Team = teamPlayer != null ? teamPlayer.Team : Team.Unassigned,
				Position = traceResult.EndPos,
				Owner = Owner,
				Rotation = Rotation.FromYaw( Rotation.Yaw() + 90f )
			}.Spawn();
		}

		public override string ToString()
		{
			return "Dispenser Builder";
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
	}
}
