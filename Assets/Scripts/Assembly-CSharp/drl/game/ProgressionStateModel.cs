using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ProgressionStateModel : Model<DRLApp>
	{
		[Serializable]
		public class LeagueData
		{
			public string guid;

			public string defaultName;

			public Texture2D thumb;

			public Texture2D thumbSmall;

			public string name
			{
				get
				{
					if (!Localization.instance)
					{
						return defaultName;
					}
					return Localization.instance.Get<string>(guid);
				}
			}
		}

		[Serializable]
		public class LevelRangeData
		{
			public int levelStart;

			public int levelEnd;

			public Texture2D thumb;
		}

		private DRLProgressionStateData m_state;

		private List<DRLProgressionTrackData> m_tracks;

		public List<LeagueData> leagues;

		public List<LevelRangeData> levelRanges = new List<LevelRangeData>
		{
			new LevelRangeData
			{
				levelStart = 1,
				levelEnd = 3
			},
			new LevelRangeData
			{
				levelStart = 4,
				levelEnd = 6
			},
			new LevelRangeData
			{
				levelStart = 7,
				levelEnd = 10
			},
			new LevelRangeData
			{
				levelStart = 11,
				levelEnd = 14
			},
			new LevelRangeData
			{
				levelStart = 15,
				levelEnd = 18
			},
			new LevelRangeData
			{
				levelStart = 19,
				levelEnd = 21
			},
			new LevelRangeData
			{
				levelStart = 22,
				levelEnd = 24
			},
			new LevelRangeData
			{
				levelStart = 25,
				levelEnd = 27
			},
			new LevelRangeData
			{
				levelStart = 28,
				levelEnd = 30
			},
			new LevelRangeData
			{
				levelStart = 31,
				levelEnd = 33
			},
			new LevelRangeData
			{
				levelStart = 34,
				levelEnd = 37
			},
			new LevelRangeData
			{
				levelStart = 38,
				levelEnd = 41
			},
			new LevelRangeData
			{
				levelStart = 42,
				levelEnd = 45
			},
			new LevelRangeData
			{
				levelStart = 45,
				levelEnd = 50
			},
			new LevelRangeData
			{
				levelStart = 51,
				levelEnd = 55
			},
			new LevelRangeData
			{
				levelStart = 56,
				levelEnd = 60
			},
			new LevelRangeData
			{
				levelStart = 61,
				levelEnd = 70
			},
			new LevelRangeData
			{
				levelStart = 71,
				levelEnd = 80
			},
			new LevelRangeData
			{
				levelStart = 81,
				levelEnd = 99
			},
			new LevelRangeData
			{
				levelStart = 100,
				levelEnd = 999
			}
		};

		private List<Sprite> m_thumb_sprites;

		private List<Sprite> m_thumb_small_sprites;

		private List<Sprite> m_levelrange_thumb_sprites;

		public PlayerStateModel parent => AssertParent<PlayerStateModel>("parent");

		public bool ready
		{
			get
			{
				if (m_state != null)
				{
					return m_state.level >= 0;
				}
				return false;
			}
		}

		public DRLProgressionStateData state
		{
			get
			{
				if (m_state != null)
				{
					return m_state;
				}
				return m_state = new DRLProgressionStateData();
			}
			set
			{
				DRLProgressionStateData dRLProgressionStateData = state;
				dRLProgressionStateData.Clear();
				if (value != null)
				{
					dRLProgressionStateData.Merge(value);
				}
			}
		}

		public List<DRLProgressionTrackData> tracks
		{
			get
			{
				if (m_tracks != null)
				{
					return m_tracks;
				}
				return m_tracks = new List<DRLProgressionTrackData>();
			}
			set
			{
				m_tracks = value;
			}
		}

		public int GetTrackXP(string p_guid)
		{
			List<DRLProgressionTrackData> list = tracks;
			for (int i = 0; i < list.Count; i++)
			{
				DRLProgressionTrackData dRLProgressionTrackData = list[i];
				if (dRLProgressionTrackData != null && !(dRLProgressionTrackData.guid != p_guid))
				{
					return dRLProgressionTrackData.xp;
				}
			}
			return 0;
		}

		public int GetTrackMinTime(string p_guid)
		{
			List<DRLProgressionTrackData> list = tracks;
			for (int i = 0; i < list.Count; i++)
			{
				DRLProgressionTrackData dRLProgressionTrackData = list[i];
				if (dRLProgressionTrackData != null && !(dRLProgressionTrackData.guid != p_guid))
				{
					return dRLProgressionTrackData.minTime;
				}
			}
			return 0;
		}

		public LeagueData GetLeagueByIndex(int p_index)
		{
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= leagues.Count)
			{
				return null;
			}
			return leagues[p_index];
		}

		public LeagueData GetLeagueByGUID(string p_guid)
		{
			return GetLeagueByIndex(GetLeagueIndexByGUID(p_guid));
		}

		public int GetLeagueIndexByGUID(string p_guid)
		{
			LeagueData leagueData = leagues.Find((LeagueData it) => it != null && it.guid == p_guid);
			if (leagueData != null)
			{
				return leagues.IndexOf(leagueData);
			}
			return -1;
		}

		public int GetLeagueIndex()
		{
			return GetLeagueIndexByGUID(state.league.guid);
		}

		public LevelRangeData GetLevelRangeByIndex(int p_index)
		{
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= levelRanges.Count)
			{
				return null;
			}
			return levelRanges[p_index];
		}

		public int GetLevelRangeIndexByLevel(int p_level)
		{
			for (int i = 0; i < levelRanges.Count; i++)
			{
				LevelRangeData levelRangeData = levelRanges[i];
				if (p_level >= levelRangeData.levelStart && p_level <= levelRangeData.levelEnd)
				{
					return i;
				}
			}
			return -1;
		}

		public int GetLevelRangeIndex()
		{
			return GetLevelRangeIndexByLevel(state.level);
		}

		public List<Texture2D> GetLeagueThumbs()
		{
			return leagues.ConvertAll((LeagueData it) => it.thumb);
		}

		public List<Texture2D> GetLeagueThumbsSmall()
		{
			return leagues.ConvertAll((LeagueData it) => it.thumbSmall);
		}

		public List<Sprite> GetLeagueThumbSprites()
		{
			if (m_thumb_sprites != null)
			{
				return m_thumb_sprites;
			}
			List<Texture2D> leagueThumbs = GetLeagueThumbs();
			return m_thumb_sprites = leagueThumbs.ConvertAll(delegate(Texture2D it)
			{
				Sprite sprite = (it ? Sprite.Create(it, new Rect(0f, 0f, it.width, it.height), Vector2.zero) : null);
				if ((bool)sprite)
				{
					sprite.name = sprite.texture.name;
				}
				return sprite;
			});
		}

		public List<Sprite> GetLeagueThumbSmallSprites()
		{
			if (m_thumb_small_sprites != null)
			{
				return m_thumb_small_sprites;
			}
			List<Texture2D> leagueThumbsSmall = GetLeagueThumbsSmall();
			return m_thumb_small_sprites = leagueThumbsSmall.ConvertAll(delegate(Texture2D it)
			{
				Sprite sprite = (it ? Sprite.Create(it, new Rect(0f, 0f, it.width, it.height), Vector2.zero) : null);
				if ((bool)sprite)
				{
					sprite.name = sprite.texture.name;
				}
				return sprite;
			});
		}

		public List<Texture2D> GetLevelRangeThumbs()
		{
			return levelRanges.ConvertAll((LevelRangeData it) => it.thumb);
		}

		public List<Sprite> GetLevelRangeThumbSprites()
		{
			if (m_levelrange_thumb_sprites != null)
			{
				return m_levelrange_thumb_sprites;
			}
			List<Texture2D> levelRangeThumbs = GetLevelRangeThumbs();
			return m_levelrange_thumb_sprites = levelRangeThumbs.ConvertAll(delegate(Texture2D it)
			{
				Sprite sprite = (it ? Sprite.Create(it, new Rect(0f, 0f, it.width, it.height), Vector2.zero) : null);
				if ((bool)sprite)
				{
					sprite.name = sprite.texture.name;
				}
				return sprite;
			});
		}

		public void Refresh(Action p_on_complete = null)
		{
			if (!base.validContext)
			{
				return;
			}
			base.app.model.service.GetPlayerProgression(delegate(DRLProgressionStateData p_result)
			{
				if (p_result != null)
				{
					state = p_result;
				}
				if (p_on_complete != null)
				{
					p_on_complete();
				}
				Notify(1f / 60f, "storage.progression@refresh");
			});
		}

		public void LoadTracks(Action p_on_complete)
		{
			if (!base.validContext)
			{
				return;
			}
			base.app.model.service.GetProgressionTracks(delegate(DRLProgressionTrackData[] p_result)
			{
				tracks = new List<DRLProgressionTrackData>();
				if (p_result != null)
				{
					tracks.AddRange(p_result);
				}
				Debug.Log("ProgressionStateModel> LoadTracks / found[" + tracks.Count + "]");
				if (p_on_complete != null)
				{
					p_on_complete();
				}
			});
		}
	}
}
