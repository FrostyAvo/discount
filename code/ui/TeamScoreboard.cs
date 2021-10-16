using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

using System.Collections.Generic;
using System.Linq;

namespace Discount.UI
{
	public partial class TeamScoreboard<T> : Panel where T : ScoreboardEntry, new()
	{
		protected Panel[] Canvases { get; set; } = new Panel[] { null, null, null, null };
		protected readonly Dictionary<Client, EntryWrapper<T>> Entries = new Dictionary<Client, EntryWrapper<T>>();

		public Panel Header { get; protected set; }

		public TeamScoreboard()
		{
			StyleSheet.Load( "/ui/TeamScoreboard.scss" );
			AddClass( "scoreboard" );

			AddTeamSection( Team.Red );
			AddTeamSection( Team.Blue );
			AddTeamSection( Team.Spectator );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Input.Down( InputButton.Score ) );

			if ( !IsVisible )
			{
				return;
			}

			// Clients that were added
			foreach ( Client client in Client.All.Except( Entries.Keys ) )
			{
				T entry = AddClient( client, (Game.Current as DiscountGame)?.Teams.GetClientTeam( client ) ?? Team.Unassigned );
				Entries[client] = new EntryWrapper<T>( entry, client );
			}

			foreach ( Client client in Entries.Keys.Except( Client.All ) )
			{
				if ( Entries.TryGetValue( client, out EntryWrapper<T> wrapper ) )
				{
					wrapper.Entry?.Delete();
					Entries.Remove( client );
				}
			}

			foreach ( EntryWrapper<T> wrapper in Entries.Values )
			{
				Team clientTeam = (Game.Current as DiscountGame)?.Teams.GetClientTeam( wrapper.Client ) ?? Team.Unassigned;

				// If team has changed
				if ( wrapper.Team != clientTeam )
				{
					wrapper.Team = clientTeam;

					wrapper.Entry?.Delete();

					wrapper.Entry = AddClient( wrapper.Client, clientTeam );
				}
			}
		}

		protected virtual T AddClient( Client client, Team team )
		{
			Panel canvas = Canvases[(int)team];

			if ( canvas == null )
			{
				return null;
			}

			T p = canvas.AddChild<T>();
			p.Client = client;

			return p;
		}

		protected void AddTeamSection( Team team )
		{
			Panel teamSection = Add.Panel( "team-section" );

			Label header = teamSection.Add.Label( Teams.GetLongTeamName( team ), "header" );
			header.Style.FontColor = Teams.GetLightTeamColor( team );

			Panel columnLabels = teamSection.Add.Panel( "column-labels" );
			columnLabels.Add.Label( "Name", "name" );
			columnLabels.Add.Label( "Kills", "kills" );
			columnLabels.Add.Label( "Deaths", "deaths" );
			columnLabels.Add.Label( "Ping", "ping" );

			Canvases[(int)team] = teamSection.Add.Panel( "canvas" );
		}

		protected class EntryWrapper<T> where T : ScoreboardEntry, new()
		{
			public T Entry;
			public Client Client;
			public Team Team;

			public EntryWrapper(T entry, Client client)
			{
				Entry = entry;
				Client = client;
				Team = (Game.Current as DiscountGame)?.Teams.GetClientTeam( client ) ?? Team.Unassigned;
			}
		}
	}
}
