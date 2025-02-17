﻿// Copyright (c) Richasy. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bili.Adapter.Interfaces;
using Bili.Models.App.Constants;
using Bili.Models.BiliBili;
using Bili.Models.Data.Appearance;
using Bili.Models.Data.Pgc;
using Bili.Models.Data.Player;
using Bili.Models.Data.Video;
using Bili.Models.Enums;
using Bili.Models.Enums.Player;
using Bili.Toolkit.Interfaces;
using Bilibili.App.Dynamic.V2;

namespace Bili.Adapter
{
    /// <summary>
    /// PGC 内容适配器.
    /// </summary>
    public sealed class PgcAdapter : IPgcAdapter
    {
        private readonly IImageAdapter _imageAdapter;
        private readonly IUserAdapter _userAdapter;
        private readonly ICommunityAdapter _communityAdapter;
        private readonly ITextToolkit _textToolkit;

        /// <summary>
        /// Initializes a new instance of the <see cref="PgcAdapter"/> class.
        /// </summary>
        /// <param name="imageAdapter">图片适配器.</param>
        /// <param name="communityAdapter">社区信息适配器.</param>
        /// <param name="userAdapter">用户适配器.</param>
        /// <param name="textToolkit">文本工具.</param>
        public PgcAdapter(
            IImageAdapter imageAdapter,
            ICommunityAdapter communityAdapter,
            IUserAdapter userAdapter,
            ITextToolkit textToolkit)
        {
            _imageAdapter = imageAdapter;
            _communityAdapter = communityAdapter;
            _userAdapter = userAdapter;
            _textToolkit = textToolkit;
        }

        /// <inheritdoc/>
        public EpisodeInformation ConvertToEpisodeInformation(PgcEpisodeDetail episode)
        {
            var epid = episode.Report.EpisodeId;
            var title = string.IsNullOrEmpty(episode.LongTitle)
                ? episode.ShareTitle
                : episode.LongTitle;
            title = _textToolkit.ConvertToTraditionalChineseIfNeeded(title);
            var subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(episode.Subtitle);
            var duration = episode.Duration / 1000;
            var cover = _imageAdapter.ConvertToVideoCardCover(episode.Cover);
            var seasonId = episode.Report.SeasonId;
            var aid = episode.Aid.ToString();
            var cid = episode.PartId.ToString();
            var index = episode.Index;
            var isPv = episode.IsPV == 1;
            var isVip = episode.BadgeText.Contains("会员");
            var publishTime = DateTimeOffset.FromUnixTimeSeconds(episode.PublishTime).ToLocalTime().DateTime;
            var communityInfo = _communityAdapter.ConvertToVideoCommunityInformation(episode.Stat);
            var seasonType = episode.Report.SeasonType;

            var identifier = new VideoIdentifier(epid, title, duration, cover);
            return new EpisodeInformation(
                identifier,
                seasonId,
                aid,
                subtitle,
                index,
                isVip,
                isPv,
                publishTime,
                communityInfo,
                episode.BadgeText,
                cid,
                seasonType);
        }

        /// <inheritdoc/>
        public EpisodeInformation ConvertToEpisodeInformation(RecommendCard card)
        {
            if (card.CardGoto != ServiceConstants.Bangumi)
            {
                throw new ArgumentException($"推荐卡片的 CardGoTo 属性应该是 {ServiceConstants.Bangumi}，这里是 {card.Goto}，不符合要求，请检查分配条件", nameof(card));
            }

            var epid = card.Parameter;
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(card.Title);
            var subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(card.Description);
            var communityInfo = _communityAdapter.ConvertToVideoCommunityInformation(card);
            var cover = _imageAdapter.ConvertToVideoCardCover(card.Cover);

            var identifier = new VideoIdentifier(epid, title, -1, cover);
            return new EpisodeInformation(identifier, subtitle: subtitle, communityInformation: communityInfo);
        }

        /// <inheritdoc/>
        public EpisodeInformation ConvertToEpisodeInformation(PgcModuleItem item)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Title);
            var epid = item.Aid.ToString();
            var ssid = item.OriginId.ToString();
            var cover = _imageAdapter.ConvertToVideoCardCover(item.Cover);
            var highlight = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Badge);
            var communityInfo = _communityAdapter.ConvertToVideoCommunityInformation(item.Stat);
            communityInfo.Id = epid;
            var subtitle = item.Stat?.FollowDisplayText ?? item.DisplayScoreText;
            subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(subtitle);

            var identifier = new VideoIdentifier(epid, title, -1, cover);
            return new EpisodeInformation(
                identifier,
                ssid,
                subtitle: subtitle,
                communityInformation: communityInfo,
                highlight: highlight);
        }

        /// <inheritdoc/>
        public EpisodeInformation ConvertToEpisodeInformation(MdlDynPGC pgc)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(pgc.Title);
            var ssid = pgc.SeasonId.ToString();
            var epid = pgc.Epid.ToString();
            var aid = pgc.Aid.ToString();
            var isPv = pgc.IsPreview;
            var cover = _imageAdapter.ConvertToVideoCardCover(pgc.Cover);
            var duration = Convert.ToInt32(pgc.Duration);

            var identifier = new VideoIdentifier(epid, title, duration, cover);
            return new EpisodeInformation(identifier, ssid, aid, isPv: isPv);
        }

        /// <inheritdoc/>
        public EpisodeInformation ConvertToEpisodeInformation(MdlDynArchive archive)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(archive.Title);
            var ssid = archive.PgcSeasonId.ToString();
            var epid = archive.EpisodeId.ToString();
            var aid = archive.Avid.ToString();
            var isPv = archive.IsPreview;
            var cover = _imageAdapter.ConvertToVideoCardCover(archive.Cover);
            var duration = Convert.ToInt32(archive.Duration);

            var identifier = new VideoIdentifier(epid, title, duration, cover);
            return new EpisodeInformation(identifier, ssid, aid, isPv: isPv);
        }

        /// <inheritdoc/>
        public SeasonInformation ConvertToSeasonInformation(PgcModuleItem item, PgcType type)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Title);
            var ssid = item.OriginId.ToString();
            var cover = _imageAdapter.ConvertToPgcCover(item.Cover);
            var tags = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.SeasonTags);
            var highlight = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Badge);
            var communityInfo = _communityAdapter.ConvertToVideoCommunityInformation(item.Stat);
            communityInfo.Id = ssid;
            var subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Description);
            var description = item.Stat?.FollowDisplayText ?? item.DisplayScoreText;
            description = _textToolkit.ConvertToTraditionalChineseIfNeeded(description);
            var isTracking = item.Status?.IsFollow == 1;

            var identifier = new VideoIdentifier(ssid, title, -1, cover);
            return new SeasonInformation(
                identifier,
                subtitle,
                highlight,
                tags,
                type: type,
                description: description,
                communityInformation: communityInfo,
                isTracking: isTracking);
        }

        /// <inheritdoc/>
        public SeasonInformation ConvertToSeasonInformation(PgcSearchItem item)
        {
            var ssid = item.SeasonId.ToString();
            var title = Regex.Replace(item.Title, "<[^>]+>", string.Empty);
            title = _textToolkit.ConvertToTraditionalChineseIfNeeded(title);
            var subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Label);
            var tags = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.SubTitle);
            var cover = _imageAdapter.ConvertToPgcCover(item.Cover);
            var highlight = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.BadgeText);
            var isTracking = item.IsFollow == 1;
            var type = GetPgcTypeFromTypeText(item.SeasonTypeName);
            var description = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Area);
            var ratingCount = Convert.ToInt32(item.VoteNumber);
            var communityInfo = _communityAdapter.ConvertToVideoCommunityInformation(item);

            var identifier = new VideoIdentifier(ssid, title, -1, cover);
            return new SeasonInformation(
                identifier,
                subtitle,
                highlight,
                tags,
                ratingCount,
                description: description,
                type: type,
                communityInformation: communityInfo,
                isTracking: isTracking);
        }

        /// <inheritdoc/>
        public SeasonInformation ConvertToSeasonInformation(PgcIndexItem item)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Title);
            var ssid = item.SeasonId.ToString();
            var tags = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.OrderText);
            var cover = _imageAdapter.ConvertToPgcCover(item.Cover);
            var highlight = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.BadgeText);
            var description = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.AdditionalText);
            var subtitle = item.IsFinish == 1
                ? "已完结"
                : "连载中";
            subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(subtitle);

            var identifier = new VideoIdentifier(ssid, title, -1, cover);
            return new SeasonInformation(
                identifier,
                subtitle,
                highlight,
                tags,
                description: description);
        }

        /// <inheritdoc/>
        public SeasonInformation ConvertToSeasonInformation(TimeLineEpisode item)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Title);
            var ssid = item.SeasonId.ToString();
            var publishTime = DateTimeOffset.FromUnixTimeSeconds(item.PublishTimeStamp).ToLocalTime().DateTime;
            var tags = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.PublishIndex);
            var cover = _imageAdapter.ConvertToPgcCover(item.Cover);
            var description = item.IsPublished == 1
                ? "已更新"
                : "待发布";
            description = _textToolkit.ConvertToTraditionalChineseIfNeeded(description);
            var subtitle = publishTime.ToString("MM/dd HH:mm");

            var identifier = new VideoIdentifier(ssid, title, -1, cover);
            return new SeasonInformation(identifier, subtitle, tags: tags, description: description);
        }

        /// <inheritdoc/>
        public SeasonInformation ConvertToSeasonInformation(PgcPlayListSeason season)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(season.Title);
            var ssid = season.SeasonId.ToString();
            var tags = _textToolkit.ConvertToTraditionalChineseIfNeeded(season.Styles);
            var subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(season.Subtitle);
            var description = _textToolkit.ConvertToTraditionalChineseIfNeeded(season.Description);
            var highlight = _textToolkit.ConvertToTraditionalChineseIfNeeded(season.BadgeText);
            var cover = _imageAdapter.ConvertToPgcCover(season.Cover);
            var communityInfo = _communityAdapter.ConvertToVideoCommunityInformation(season.Stat);
            communityInfo.Id = ssid;
            if (season.Rating != null)
            {
                communityInfo.Score = season.Rating.Score;
            }

            var identifier = new VideoIdentifier(ssid, title, -1, cover);
            return new SeasonInformation(
                identifier,
                subtitle,
                highlight,
                tags,
                description: description,
                communityInformation: communityInfo);
        }

        /// <inheritdoc/>
        public SeasonInformation ConvertToSeasonInformation(FavoritePgcItem item)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Title);
            var subtitle = item.NewEpisode?.DisplayText ?? string.Empty;
            subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(subtitle);
            var ssid = item.SeasonId.ToString();
            var type = GetPgcTypeFromTypeText(item.SeasonTypeName);
            var cover = _imageAdapter.ConvertToPgcCover(item.Cover);
            var highlight = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.BadgeText);
            var description = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.SeasonTypeName);

            var identifier = new VideoIdentifier(ssid, title, -1, cover);
            return new SeasonInformation(
                identifier,
                subtitle,
                highlight,
                type: type,
                description: description);
        }

        /// <inheritdoc/>
        public PgcPlayerView ConvertToPgcPlayerView(PgcDisplayInformation display)
        {
            var seasonInfo = GetSeasonInformationFromDisplayInformation(display);
            List<VideoIdentifier> seasons = null;
            List<EpisodeInformation> episodes = null;
            Dictionary<string, IEnumerable<EpisodeInformation>> extras = null;
            PlayedProgress history = null;

            if (display.Modules != null)
            {
                var seasonModule = display.Modules.FirstOrDefault(p => p.Style == ServiceConstants.Season);
                if (seasonModule != null)
                {
                    seasons = new List<VideoIdentifier>();
                    foreach (var item in seasonModule.Data.Seasons)
                    {
                        var cover = _imageAdapter.ConvertToImage(item.Cover, 240, 320);
                        seasons.Add(new VideoIdentifier(item.SeasonId.ToString(), item.Title, -1, cover));
                    }
                }

                var episodeModule = display.Modules.FirstOrDefault(p => p.Style == ServiceConstants.Positive);
                if (episodeModule != null)
                {
                    episodes = episodeModule.Data.Episodes
                        .Select(p => ConvertToEpisodeInformation(p))
                        .ToList();
                }

                var sectionModules = display.Modules.Where(p => p.Style == ServiceConstants.Section);
                if (sectionModules.Any())
                {
                    extras = new Dictionary<string, IEnumerable<EpisodeInformation>>();
                    foreach (var section in sectionModules)
                    {
                        if (section.Data?.Episodes?.Any() ?? false)
                        {
                            extras.Add(section.Title, section.Data.Episodes.Select(p => ConvertToEpisodeInformation(p)));
                        }
                    }
                }
            }

            if (display.UserStatus?.Progress != null && episodes != null)
            {
                var progress = display.UserStatus.Progress;
                var historyEpid = progress.LastEpisodeId.ToString();
                var historyEp = episodes.Any(p => p.Identifier.Id == historyEpid)
                    ? episodes.FirstOrDefault(p => p.Identifier.Id == historyEpid)
                    : episodes.FirstOrDefault(p => p.Index.ToString() == progress.LastEpisodeIndex);

                if (historyEp != null)
                {
                    var status = progress.LastTime switch
                    {
                        -1 => PlayedProgressStatus.Finish,
                        0 => PlayedProgressStatus.NotStarted,
                        _ => PlayedProgressStatus.Playing
                    };
                    history = new PlayedProgress(progress.LastTime, status, historyEp.Identifier);
                }
            }

            var warning = display.Warning?.Message;

            return new PgcPlayerView(seasonInfo, seasons, episodes, extras, history, warning);
        }

        /// <inheritdoc/>
        public PgcPlaylist ConvertToPgcPlaylist(PgcModule module)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(module.Title);
            var id = string.Empty;
            if (module.Headers?.Count > 0)
            {
                var header = module.Headers.First();
                if (header.Url.Contains("/sl"))
                {
                    var uri = new Uri(header.Url);
                    id = uri.Segments.Where(p => p.Contains("sl"))
                        .Select(p => p.Replace("sl", string.Empty))
                        .FirstOrDefault();
                }
            }

            var seasons = module.Items.Select(p => ConvertToSeasonInformation(p, PgcType.Bangumi | PgcType.Domestic));
            return new PgcPlaylist(title, id, seasons: seasons);
        }

        /// <inheritdoc/>
        public PgcPlaylist ConvertToPgcPlaylist(PgcPlayListResponse response)
        {
            var id = response.Id.ToString();
            var subtitle = $"{response.Total} · {response.Description}";
            subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(subtitle);
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(response.Title);
            var seasons = response.Seasons.Select(p => ConvertToSeasonInformation(p));
            return new PgcPlaylist(title, id, subtitle, seasons);
        }

        /// <inheritdoc/>
        public PgcPageView ConvertToPgcPageView(PgcResponse response)
        {
            var banners = response.Modules.Where(p => p.Style.Contains("banner"))
                .SelectMany(p => p.Items)
                .Select(p => _communityAdapter.ConvertToBannerIdentifier(p));

            var originRanks = response.Modules.Where(p => p.Style.Contains("rank"))
                .SelectMany(p => p.Items);

            var ranks = new Dictionary<string, IEnumerable<EpisodeInformation>>();
            foreach (var item in originRanks)
            {
                var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(item.Title);
                var subRanks = item.Cards.Take(3).Select(p => ConvertToEpisodeInformation(p)).ToList();
                ranks.Add(title, subRanks);
            }

            var partitionId = response.FeedIdentifier == null
                ? string.Empty
                : response.FeedIdentifier.PartitionIds.First().ToString();

            var playLists = response.Modules.Where(p => p.Style == "v_card" || p.Style.Contains("feed"))
                .Select(p => ConvertToPgcPlaylist(p))
                .ToList();

            var seasons = response.Modules.Where(p => p.Style == "common_feed")
                .SelectMany(p => p.Items)
                .Select(p => ConvertToSeasonInformation(p, PgcType.Documentary | PgcType.Movie | PgcType.TV))
                .ToList();

            return new PgcPageView(banners, ranks, playLists, seasons, partitionId);
        }

        /// <inheritdoc/>
        public IEnumerable<Filter> ConvertToFilters(PgcIndexConditionResponse response)
        {
            var filters = response.FilterList
                .Select(p => GetFilterFromIndexFilter(p))
                .ToList();
            if (response.OrderList?.Count > 0)
            {
                var name = _textToolkit.ConvertToTraditionalChineseIfNeeded("排序方式");
                var id = "order";
                var conditions = response.OrderList.Select(p => new Condition(_textToolkit.ConvertToTraditionalChineseIfNeeded(p.Name), p.Field)).ToList();
                filters.Insert(0, new Filter(name, id, conditions));
            }

            return filters;
        }

        /// <inheritdoc/>
        public TimelineView ConvertToTimelineView(PgcTimeLineResponse response)
        {
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(response.Title);
            var desc = _textToolkit.ConvertToTraditionalChineseIfNeeded(response.Subtitle);
            var timelines = new List<TimelineInformation>();
            foreach (var item in response.Data)
            {
                var seasons = item.Episodes?.Count > 0
                    ? item.Episodes.Select(p => ConvertToSeasonInformation(p)).ToList()
                    : null;
                var info = new TimelineInformation(item.Date, ConvertDayOfWeek(item.DayOfWeek), item.IsToday == 1, seasons);
                timelines.Add(info);
            }

            return new TimelineView(title, desc, timelines);
        }

        /// <inheritdoc/>
        public SeasonSet ConvertToSeasonSet(PgcFavoriteListResponse response)
        {
            var total = response.Total;
            var items = response.FollowList != null
                ? response.FollowList.Select(p => ConvertToSeasonInformation(p))
                : new List<SeasonInformation>();
            return new SeasonSet(items, total);
        }

        private SeasonInformation GetSeasonInformationFromDisplayInformation(PgcDisplayInformation display)
        {
            var ssid = display.SeasonId.ToString();
            var title = _textToolkit.ConvertToTraditionalChineseIfNeeded(display.Title);
            var cover = _imageAdapter.ConvertToPgcCover(display.Cover);
            var subtitle = _textToolkit.ConvertToTraditionalChineseIfNeeded(display.Subtitle);
            var description = _textToolkit.ConvertToTraditionalChineseIfNeeded(display.Evaluate);
            var highlight = _textToolkit.ConvertToTraditionalChineseIfNeeded(display.BadgeText);
            var progress = display.PublishInformation.DisplayProgress;
            var publishDate = display.PublishInformation.DisplayPublishTime;
            var originName = display.OriginName;
            var alias = display.Alias;
            var tags = $"{display.TypeDescription}\n" +
                $"{display.PublishInformation.DisplayReleaseDate}\n" +
                $"{display.PublishInformation.DisplayProgress}";
            tags = _textToolkit.ConvertToTraditionalChineseIfNeeded(tags);
            var isTracking = display.UserStatus.IsFollow == 1;
            var ratingCount = display.Rating != null
                ? display.Rating.Count
                : -1;
            var labors = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(display.Actor?.Information))
            {
                labors.Add(
                    _textToolkit.ConvertToTraditionalChineseIfNeeded(display.Actor.Title),
                    _textToolkit.ConvertToTraditionalChineseIfNeeded(display.Actor.Information));
            }

            if (!string.IsNullOrEmpty(display.Staff?.Information))
            {
                labors.Add(
                    _textToolkit.ConvertToTraditionalChineseIfNeeded(display.Staff.Title),
                    _textToolkit.ConvertToTraditionalChineseIfNeeded(display.Staff.Information));
            }

            var celebrities = display.Celebrity?.Select(p => _userAdapter.ConvertToRoleProfile(p));
            var communityInfo = _communityAdapter.ConvertToVideoCommunityInformation(display.InformationStat);
            communityInfo.Id = ssid;
            if (display.Rating != null)
            {
                communityInfo.Score = display.Rating.Score;
            }

            var type = GetPgcTypeFromTypeText(display.TypeName);

            var identifier = new VideoIdentifier(ssid, title, -1, cover);
            return new SeasonInformation(
                identifier,
                subtitle,
                highlight,
                tags,
                ratingCount,
                originName,
                alias,
                publishDate,
                progress,
                description,
                type,
                labors,
                communityInfo,
                celebrities,
                isTracking);
        }

        private PgcType GetPgcTypeFromTypeText(string typeText)
        {
            return typeText switch
            {
                "国创" => PgcType.Domestic,
                "电影" => PgcType.Movie,
                "电视剧" => PgcType.TV,
                "纪录片" => PgcType.Documentary,
                _ => PgcType.Bangumi,
            };
        }

        private Filter GetFilterFromIndexFilter(PgcIndexFilter filter)
        {
            var conditions = filter.Values.Select(p => new Condition(p.Name, p.Keyword));
            return new Filter(filter.Name, filter.Field, conditions);
        }

        private string ConvertDayOfWeek(int day)
        {
            var dayOfWeek = day switch
            {
                1 => "一",
                2 => "二",
                3 => "三",
                4 => "四",
                5 => "五",
                6 => "六",
                7 => "日",
                _ => "-",
            };

            return _textToolkit.ConvertToTraditionalChineseIfNeeded($"周{dayOfWeek}");
        }
    }
}
