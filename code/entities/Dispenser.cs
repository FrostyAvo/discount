using Sandbox;

namespace Discount
{
	public partial class Dispenser : TeamBuilding
	{
		[Net]
		protected ResupplyField ResupplyField { get; set; }

		public override string Model => "models/rust_props/electrical_boxes/electrical_box_b.vmdl";
		public override float ModelScale => 0.8f;

		public Dispenser()
		{
			
		}

		public override void Spawn()
		{
			base.Spawn();

			if ( IsServer )
			{
				(Owner as TeamPlayer)?.AddOwnedBuilding( this );

				if ( ResupplyField == null )
				{
					ResupplyField = new ResupplyField( Team )
					{
						Parent = this,
						Position = Position + Vector3.Up * 30f
					};

					ResupplyField.Spawn();
				}
				else
				{
					ResupplyField.Team = Team;
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( IsServer )
			{
				ResupplyField?.Delete();
			}
		}
	}
}
