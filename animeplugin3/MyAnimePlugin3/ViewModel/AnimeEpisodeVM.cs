﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BinaryNorthwest;

namespace MyAnimePlugin3.ViewModel
{
	public class AnimeEpisodeVM
	{
		public int AnimeEpisodeID { get; set; }
		public int EpisodeNumber { get; set; }
		public int EpisodeType { get; set; }
		public int AnimeSeriesID { get; set; }
		public int AniDB_EpisodeID { get; set; }
		public string Description { get; set; }
		public DateTime DateTimeUpdated { get; set; }
		public int IsWatched { get; set; }
		public DateTime? WatchedDate { get; set; }
		public int PlayedCount { get; set; }
		public int WatchedCount { get; set; }
		public int StoppedCount { get; set; }
		public int LocalFileCount { get; set; }
		public int UnwatchedEpCountSeries { get; set; }


		public int AniDB_LengthSeconds { get; set; }
		public string AniDB_Rating { get; set; }
		public string AniDB_Votes { get; set; }
		public string AniDB_RomajiName { get; set; }
		public string AniDB_EnglishName { get; set; }
		public DateTime? AniDB_AirDate { get; set; }

		public string EpisodeOverview { get; set; }
		public string EpisodeImageLocation { get; set; }

		private AnimeSeriesVM animeSeries = null;
		public AnimeSeriesVM AnimeSeries
		{
			get
			{
				if (animeSeries == null)
					animeSeries = JMMServerHelper.GetSeries(this.AnimeSeriesID);
		
				return animeSeries;
			}
		}

		public enEpisodeType EpisodeTypeEnum
		{
			get
			{
				return (enEpisodeType)EpisodeType;
			}
		}

		public string DefaultAudioLanguage
		{
			get
			{
				if (AnimeSeries == null) return string.Empty;
				return AnimeSeries.DefaultAudioLanguage;
			}
		}

		public string DefaultSubtitleLanguage
		{
			get
			{
				if (AnimeSeries == null) return string.Empty;
				return AnimeSeries.DefaultSubtitleLanguage;
			}
		}

		public bool Watched
		{
			get { return IsWatched == 1; }
		}

		public bool MultipleUnwatchedEpsSeries
		{
			get { return UnwatchedEpCountSeries > 1; }
		}

		public string RunTime
		{
			get
			{
				return Utils.FormatSecondsToDisplayTime(AniDB_LengthSeconds);
			}
		}

		public string EpisodeName { get ; set;}

		public string EpisodeNumberAndName
		{
			get
			{
				return string.Format("{0} - {1}", EpisodeNumber, EpisodeName);
			}
		}

		public string EpisodeNumberAndNameWithType
		{
			get
			{
				return string.Format("{0}{1} - {2}", ShortType, EpisodeNumber, EpisodeName);
			}
		}

		public string EpisodeTypeAndNumber
		{
			get
			{
				return  string.Format("{0}{1}", ShortType, EpisodeNumber);
			}
		}

		public string EpisodeTypeAndNumberAbsolute
		{
			get
			{
				return string.Format("{0}{1}", ShortType, EpisodeNumber.ToString().PadLeft(5, '0'));
			}
		}

		public string ShortType
		{
			get
			{
				string shortType = "";
				switch (EpisodeTypeEnum)
				{
					case enEpisodeType.Credits: shortType = "C"; break;
					case enEpisodeType.Episode: shortType = ""; break;
					case enEpisodeType.Other: shortType = "O"; break;
					case enEpisodeType.Parody: shortType = "P"; break;
					case enEpisodeType.Special: shortType = "S"; break;
					case enEpisodeType.Trailer: shortType = "T"; break;
				}
				return shortType;
			}
		}

		public string AirDateAsString
		{
			get
			{
				if (AniDB_AirDate.HasValue)
					return AniDB_AirDate.Value.ToString("dd MMM yyyy", Globals.Culture);
				else
					return "";
			}
		}

		public string AniDBRatingFormatted
		{
			get
			{
				return string.Format("{0}: {1} ({2} {3})", "Rating", AniDB_Rating, AniDB_Votes, "Votes");
			}
		}

		public bool FutureDated
		{
			get
			{
				if (!AniDB_AirDate.HasValue) return true;

				return (AniDB_AirDate.Value > DateTime.Now);
			}
		}


		public string AniDB_SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.AniDB_Episode, AniDB_EpisodeID);
			}
		}

		public AnimeEpisodeVM()
		{
		}

		public AnimeEpisodeVM(JMMServerBinary.Contract_AnimeEpisode contract)
		{
			Populate(contract);
		}

		public void Populate(JMMServerBinary.Contract_AnimeEpisode contract)
		{
			try
			{
				//Cloner.Clone(contract, this);
				this.AniDB_EpisodeID = contract.AniDB_EpisodeID;
				this.AnimeEpisodeID = contract.AnimeEpisodeID;
				this.AnimeSeriesID = contract.AnimeSeriesID;
				this.DateTimeUpdated = contract.DateTimeUpdated;
				this.Description = "";
				this.EpisodeNumber = contract.EpisodeNumber;
				this.EpisodeType = contract.EpisodeType;
				this.IsWatched = contract.IsWatched;
				this.LocalFileCount = contract.LocalFileCount;
				this.PlayedCount = contract.PlayedCount;
				this.StoppedCount = contract.StoppedCount;
				this.WatchedCount = contract.WatchedCount;
				this.WatchedDate = contract.WatchedDate;
				this.UnwatchedEpCountSeries = contract.UnwatchedEpCountSeries;

				this.AniDB_LengthSeconds = contract.AniDB_LengthSeconds;
				this.AniDB_Rating = contract.AniDB_Rating;
				this.AniDB_Votes = contract.AniDB_Votes;
				this.AniDB_RomajiName = contract.AniDB_RomajiName;
				this.AniDB_EnglishName = contract.AniDB_EnglishName;
				this.AniDB_AirDate = contract.AniDB_AirDate;

				this.EpisodeOverview = "";
				this.EpisodeImageLocation = "";

				if (AniDB_EnglishName.Trim().Length > 0)
					EpisodeName = AniDB_EnglishName;
				else
					EpisodeName = AniDB_RomajiName;

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}

		public void SetTvDBInfo(TvDBSummary tvSummary)
		{
			this.EpisodeOverview = "Episode Overview Not Available";
			this.EpisodeImageLocation = "";

			#region episode override
			// check if this episode has a direct tvdb over-ride
			if (tvSummary.DictTvDBCrossRefEpisodes.ContainsKey(AniDB_EpisodeID))
			{
				foreach (TvDB_EpisodeVM tvep in tvSummary.DictTvDBEpisodes.Values)
				{
					if (tvSummary.DictTvDBCrossRefEpisodes[AniDB_EpisodeID] == tvep.Id)
					{
						if (string.IsNullOrEmpty(tvep.Overview))
							this.EpisodeOverview = "Episode Overview Not Available";
						else
							this.EpisodeOverview = tvep.Overview;

						if (string.IsNullOrEmpty(tvep.FullImagePath) || !File.Exists(tvep.FullImagePath))
						{
							if (string.IsNullOrEmpty(tvep.OnlineImagePath))
								this.EpisodeImageLocation = @"/Images/EpisodeThumb_NotFound.png";
							else
								this.EpisodeImageLocation = tvep.OnlineImagePath;
						}
						else
							this.EpisodeImageLocation = tvep.FullImagePath;

						if (JMMServerVM.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
							EpisodeName = tvep.EpisodeName;

						return;
					}
				}
			}
			#endregion

			#region normal episodes
			// now do stuff to improve performance
			if (this.EpisodeTypeEnum == enEpisodeType.Episode)
			{
				if (tvSummary != null && tvSummary.CrossRefTvDBV2 != null && tvSummary.CrossRefTvDBV2.Count > 0)
				{
					// find the xref that is right
					// relies on the xref's being sorted by season number and then episode number (desc)
					List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
					sortCriteria.Add(new SortPropOrFieldAndDirection("AniDBStartEpisodeNumber", true, SortType.eInteger));
					List<CrossRef_AniDB_TvDBVMV2> tvDBCrossRef = Sorting.MultiSort<CrossRef_AniDB_TvDBVMV2>(tvSummary.CrossRefTvDBV2, sortCriteria);

					bool foundStartingPoint = false;
					CrossRef_AniDB_TvDBVMV2 xrefBase = null;
					foreach (CrossRef_AniDB_TvDBVMV2 xrefTV in tvDBCrossRef)
					{
						if (xrefTV.AniDBStartEpisodeType != (int)enEpisodeType.Episode) continue;
						if (this.EpisodeNumber >= xrefTV.AniDBStartEpisodeNumber)
						{
							foundStartingPoint = true;
							xrefBase = xrefTV;
							break;
						}
					}

					// we have found the starting epiosde numbder from AniDB
					// now let's check that the TvDB Season and Episode Number exist
					if (foundStartingPoint)
					{

						Dictionary<int, int> dictTvDBSeasons = null;
						Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = null;
						foreach (TvDBDetails det in tvSummary.TvDetails.Values)
						{
							if (det.TvDBID == xrefBase.TvDBID)
							{
								dictTvDBSeasons = det.DictTvDBSeasons;
								dictTvDBEpisodes = det.DictTvDBEpisodes;
								break;
							}
						}

						if (dictTvDBSeasons.ContainsKey(xrefBase.TvDBSeasonNumber))
						{
							int episodeNumber = dictTvDBSeasons[xrefBase.TvDBSeasonNumber] + (this.EpisodeNumber + xrefBase.TvDBStartEpisodeNumber - 2) - (xrefBase.AniDBStartEpisodeNumber - 1);
							if (dictTvDBEpisodes.ContainsKey(episodeNumber))
							{

								TvDB_EpisodeVM tvep = dictTvDBEpisodes[episodeNumber];
								if (string.IsNullOrEmpty(tvep.Overview))
									this.EpisodeOverview = "Episode Overview Not Available";
								else
									this.EpisodeOverview = tvep.Overview;

								if (string.IsNullOrEmpty(tvep.FullImagePath) || !File.Exists(tvep.FullImagePath))
								{
									if (string.IsNullOrEmpty(tvep.OnlineImagePath))
										this.EpisodeImageLocation = @"/Images/EpisodeThumb_NotFound.png";
									else
										this.EpisodeImageLocation = tvep.OnlineImagePath;
								}
								else
									this.EpisodeImageLocation = tvep.FullImagePath;

								if (JMMServerVM.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
									EpisodeName = tvep.EpisodeName;
							}
						}
					}
				}
			}
			#endregion


			#region special episodes
			if (this.EpisodeTypeEnum == enEpisodeType.Special)
			{
				// find the xref that is right
				// relies on the xref's being sorted by season number and then episode number (desc)
				List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
				sortCriteria.Add(new SortPropOrFieldAndDirection("AniDBStartEpisodeNumber", true, SortType.eInteger));
				List<CrossRef_AniDB_TvDBVMV2> tvDBCrossRef = Sorting.MultiSort<CrossRef_AniDB_TvDBVMV2>(tvSummary.CrossRefTvDBV2, sortCriteria);

				bool foundStartingPoint = false;
				CrossRef_AniDB_TvDBVMV2 xrefBase = null;
				foreach (CrossRef_AniDB_TvDBVMV2 xrefTV in tvDBCrossRef)
				{
					if (xrefTV.AniDBStartEpisodeType != (int)enEpisodeType.Special) continue;
					if (this.EpisodeNumber >= xrefTV.AniDBStartEpisodeNumber)
					{
						foundStartingPoint = true;
						xrefBase = xrefTV;
						break;
					}
				}

				if (tvSummary != null && tvSummary.CrossRefTvDBV2 != null && tvSummary.CrossRefTvDBV2.Count > 0)
				{
					// we have found the starting epiosde numbder from AniDB
					// now let's check that the TvDB Season and Episode Number exist
					if (foundStartingPoint)
					{

						Dictionary<int, int> dictTvDBSeasons = null;
						Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = null;
						foreach (TvDBDetails det in tvSummary.TvDetails.Values)
						{
							if (det.TvDBID == xrefBase.TvDBID)
							{
								dictTvDBSeasons = det.DictTvDBSeasons;
								dictTvDBEpisodes = det.DictTvDBEpisodes;
								break;
							}
						}

						if (dictTvDBSeasons.ContainsKey(xrefBase.TvDBSeasonNumber))
						{
							int episodeNumber = dictTvDBSeasons[xrefBase.TvDBSeasonNumber] + (this.EpisodeNumber + xrefBase.TvDBStartEpisodeNumber - 2) - (xrefBase.AniDBStartEpisodeNumber - 1);
							if (dictTvDBEpisodes.ContainsKey(episodeNumber))
							{
								TvDB_EpisodeVM tvep = dictTvDBEpisodes[episodeNumber];
								this.EpisodeOverview = tvep.Overview;

								if (string.IsNullOrEmpty(tvep.FullImagePath) || !File.Exists(tvep.FullImagePath))
								{
									if (string.IsNullOrEmpty(tvep.OnlineImagePath))
										this.EpisodeImageLocation = @"/Images/EpisodeThumb_NotFound.png";
									else
										this.EpisodeImageLocation = tvep.OnlineImagePath;
								}
								else
									this.EpisodeImageLocation = tvep.FullImagePath;

								if (JMMServerVM.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
									EpisodeName = tvep.EpisodeName;
							}
						}
					}
				}
			}
			#endregion

		}

		public void RefreshFilesForEpisode()
		{
			try
			{
				filesForEpisode = new List<VideoDetailedVM>();
				List<JMMServerBinary.Contract_VideoDetailed> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesForEpisode(AnimeEpisodeID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);

				foreach (JMMServerBinary.Contract_VideoDetailed fi in contracts)
				{
					filesForEpisode.Add(new VideoDetailedVM(fi));
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}

		private List<VideoDetailedVM> filesForEpisode = null;
		public List<VideoDetailedVM> FilesForEpisode
		{
			get
			{
				/*if (filesForEpisode == null)
				{
					RefreshFilesForEpisode();
				}*/

				List<VideoDetailedVM> filesForEpisode = new List<VideoDetailedVM>();
				List<JMMServerBinary.Contract_VideoDetailed> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesForEpisode(AnimeEpisodeID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);

				foreach (JMMServerBinary.Contract_VideoDetailed fi in contracts)
				{
					filesForEpisode.Add(new VideoDetailedVM(fi));
				}  

				return filesForEpisode;
			}
		}

		public string DisplayName
		{
			get
			{
				AnimePluginSettings settings = new AnimePluginSettings();
				string newName = settings.EpisodeDisplayFormat;

				if (newName.Contains(Constants.EpisodeDisplayString.EpisodeNumber))
					newName = newName.Replace(Constants.EpisodeDisplayString.EpisodeNumber, EpisodeNumber.ToString());

				if (newName.Contains(Constants.EpisodeDisplayString.EpisodeName))
					newName = newName.Replace(Constants.EpisodeDisplayString.EpisodeName, EpisodeName);


				return newName;
			}
		}

		public void ToggleWatchedStatus(bool watched)
		{
			ToggleWatchedStatus(watched, true);
		}

		public void ToggleWatchedStatus(bool watched, bool promptForRating)
		{
			bool currentStatus = IsWatched == 1;
			if (currentStatus == watched) return;

			JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(AnimeEpisodeID, watched,
				JMMServerVM.Instance.CurrentUser.JMMUserID);
			if (!string.IsNullOrEmpty(response.ErrorMessage))
			{
				BaseConfig.MyAnimeLog.Write("Error in ToggleWatchedStatus: " + response.ErrorMessage);
				return;
			}

			if (promptForRating && BaseConfig.Settings.DisplayRatingDialogOnCompletion)
			{
				JMMServerBinary.Contract_AnimeSeries contract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(response.AnimeEpisode.AnimeSeriesID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);
				if (contract != null)
				{
					AnimeSeriesVM ser = new AnimeSeriesVM(contract);
					Utils.PromptToRateSeriesOnCompletion(ser);
				}
			}
		}

		public void IncrementEpisodeStats(StatCountType statCountType)
		{
			JMMServerVM.Instance.clientBinaryHTTP.IncrementEpisodeStats(this.AnimeEpisodeID, JMMServerVM.Instance.CurrentUser.JMMUserID, 
				(int)statCountType);
		}
	}
}
