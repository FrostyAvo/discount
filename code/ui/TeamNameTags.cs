using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Discount.UI
{
	public class BaseTeamNameTag : Panel
	{
		public Label NameLabel;
		public Image Avatar;
		public Player Player;

		public BaseTeamNameTag( Player player )
		{
			Player = player;

			Client client = player?.GetClientOwner();

			if (client != null)
			{
				NameLabel = Add.Label( $"{client.Name}" );
				Avatar = Add.Image( $"avatar:{client.SteamId}" );
			}
			else
			{
				NameLabel = Add.Label( "Bot" );
				Avatar = Add.Image();
			}
		}

		public virtual void UpdateFromPlayer( Player player )
		{
			// Nothing to do unless we're showing health and shit
		}
	}

	public class TeamNameTags : Panel
	{
		Dictionary<Player, BaseTeamNameTag> ActiveTags = new Dictionary<Player, BaseTeamNameTag>();

		public float MaxDrawDistance = 400;
		public int MaxTagsToShow = 5;

		public TeamNameTags()
		{
			StyleSheet.Load( "/ui/TeamNameTags.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			List<Player> deleteList = new List<Player>();

			deleteList.AddRange( ActiveTags.Keys );

			int count = 0;

			foreach ( Player player in Entity.All.OfType<Player>().OrderBy( x => Vector3.DistanceBetween( x.Position, CurrentView.Position ) ) )
			{
				if ( UpdateNameTag( player ) )
				{
					deleteList.Remove( player );
					count++;
				}

				if ( count >= MaxTagsToShow )
				{
					break;
				}
			}

			foreach( Player player in deleteList )
			{
				ActiveTags[player].Delete();
				ActiveTags.Remove( player );
			}
		}

		public virtual BaseTeamNameTag CreateNameTag( Player player )
		{
			BaseTeamNameTag tag = new BaseTeamNameTag( player );

			tag.Parent = this;

			return tag;
		}

		public bool UpdateNameTag( Player player )
		{
			// Don't draw local player
			if ( player == Local.Pawn )
			{
				return false;
			}

			if ( player.LifeState != LifeState.Alive )
			{
				return false;
			}

			// If the local player is in a team, don't draw enemy name tags
			if ( Local.Pawn is TeamPlayer localPlayer && ( player is not TeamPlayer teamPlayer || localPlayer.Team != teamPlayer.Team ) )
			{
				return false;
			}

			//
			// Where we putting the label, in world coords
			//
			Transform head = new Transform( player.EyePos );
			Vector3 labelPos = head.Position + head.Rotation.Up * 5;

			//
			// Are we too far away?
			//
			float dist = labelPos.Distance( CurrentView.Position );

			if ( dist > MaxDrawDistance )
			{
				return false;
			}

			//
			// Are we looking in this direction?
			//
			Vector3 lookDir = (labelPos - CurrentView.Position).Normal;

			if ( CurrentView.Rotation.Forward.Dot( lookDir ) < 0.5 )
			{
				return false;
			}
				
			// TODO - can we see them

			MaxDrawDistance = 400;

			// Max Draw Distance

			float alpha = dist.LerpInverse( MaxDrawDistance, MaxDrawDistance * 0.1f, true );

			// If I understood this I'd make it proper function
			float objectSize = 0.05f / dist / (2.0f * MathF.Tan( (CurrentView.FieldOfView / 2.0f).DegreeToRadian() )) * 1500.0f;

			objectSize = objectSize.Clamp( 0.05f, 1.0f );

			if ( !ActiveTags.TryGetValue( player, out BaseTeamNameTag tag ) )
			{
				tag = CreateNameTag( player );
				ActiveTags[player] = tag;
			}

			tag.UpdateFromPlayer( player );

			Vector3 screenPos = labelPos.ToScreen();

			tag.Style.Left = Length.Fraction( screenPos.x );
			tag.Style.Top = Length.Fraction( screenPos.y );
			tag.Style.Opacity = alpha;

			PanelTransform transform = new PanelTransform();
			transform.AddTranslateY( Length.Fraction( -1.0f ) );
			transform.AddScale( objectSize );
			transform.AddTranslateX( Length.Fraction( -0.5f ) );

			tag.Style.Transform = transform;
			tag.Style.Dirty();

			return true;
		}
	}
}
