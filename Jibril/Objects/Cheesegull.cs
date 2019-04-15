using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Jibril.Helpers;
using Newtonsoft.Json;
using Shared.Enums;
using Shared.Interfaces;
using Shared.Models;
using Shared.Services;

namespace Jibril.Objects
{
	public struct CheesegullBeatmapSet
	{
		public int SetID;
		public List<CheesegullBeatmap> ChildrenBeatmaps;
		public int RankedStatus;
		public string ApprovedDate;
		public string LastUpdate;
		public string LastChecked;
		public string Artist;
		public string Title;
		public string Creator;
		public string Source;
		public string Tags;
		public bool HasVideo;
		public int Genre;
		public int Language;
		public int Favourites;
	}

	public struct CheesegullBeatmap
	{
		public int BeatmapID;
		public int ParentSetID;
		public string DiffName;
		public string FileMD5;
		public PlayMode Mode;
		public float BPM;
		public float AR;
		public float OD;
		public float CS;
		public float HP;
		public int TotalLength;
		public int HitLength;
		public int Playcount;
		public int Passcount;
		public int MaxCombo;
		public double DifficultyRating;
	}
	
    public class Cheesegull
    {
	    private readonly Config _cfg;
	    private List<CheesegullBeatmapSet> _sets;
	    private string _query;

	    private static int CheeseStatus(int status)
        {
            switch (status)
            {
                case 0:
	                return 1;
                case 2:
	                return 0;
                case 3:
	                return 3;
                case 4:
	                return -100;
                case 5:
	                return -2;
                case 7:
	                return 2;
                case 8:
	                return 4;
                default:
	                return 1;
            }
        }

        public Cheesegull(Config cfg)
        {
	        _cfg = cfg;
        }

        public void Search(string query, int rankedStatus, int playMode, int page)
        {
	        query = query.ToLower();
	        if (query.Contains("newest") || query.Contains("top rated") || query.Contains("most played"))
		        query = "";

	        rankedStatus = CheeseStatus(rankedStatus);

	        string pm;
	        if (playMode < 3 || playMode > 3)
		        pm  = "";
	        else pm = playMode.ToString();
	        
	        query = $"?mode=${pm}&amount={100}&offset={page*100}&status={rankedStatus}&query={query}";
	        string request_url = _cfg.Server.Cheesegull + "/api/search" + query;

	        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(request_url);
	        request.AutomaticDecompression = DecompressionMethods.GZip;

	        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
	        using (Stream stream = response.GetResponseStream())
	        using (StreamReader reader = new StreamReader(stream ?? throw new Exception("Request Failed!")))
	        {
		        string result = reader.ReadToEnd();
		        _sets = JsonConvert.DeserializeObject<List<CheesegullBeatmapSet>>(result);
	        }

	        _query = query;
        }

        public void SetBMSet(int SetId)
        {
	        string request_url = _cfg.Server.Cheesegull + "/api/s/" + SetId;

	        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(request_url);
	        request.AutomaticDecompression = DecompressionMethods.GZip;

	        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
	        using (Stream stream = response.GetResponseStream())
	        using (StreamReader reader = new StreamReader(stream ?? throw new Exception("Request Failed!")))
	        {
		        string result = reader.ReadToEnd();
		        _sets = new List<CheesegullBeatmapSet>(new[]
		        {
			        JsonConvert.DeserializeObject<CheesegullBeatmapSet>(result)
		        });
	        }
        }

        public void SetBM(int BeatmapId)
        {
	        string request_url = _cfg.Server.Cheesegull + "/api/b/" + BeatmapId;

	        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(request_url);
	        request.AutomaticDecompression = DecompressionMethods.GZip;

	        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
	        using (Stream stream = response.GetResponseStream())
	        using (StreamReader reader = new StreamReader(stream ?? throw new Exception("Request Failed!")))
	        {
		        string result = reader.ReadToEnd();
		        SetBMSet(JsonConvert.DeserializeObject<CheesegullBeatmap>(result).ParentSetID);
	        }
        }

        public void SetBM(string Hash)
        {
	        string request_url = _cfg.Server.Cheesegull + "/api/hash/" + Hash;

	        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(request_url);
	        request.AutomaticDecompression = DecompressionMethods.GZip;

	        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
	        using (Stream stream = response.GetResponseStream())
	        using (StreamReader reader = new StreamReader(stream ?? throw new Exception("Request Failed!")))
	        {
		        string result = reader.ReadToEnd();
		        SetBMSet(JsonConvert.DeserializeObject<CheesegullBeatmap>(result).ParentSetID);
	        }
        }
        
        public string ToDirect()
        {
	        string RetStr = string.Empty;

	        if (_sets.Count >= 100)
		        RetStr += "101";
	        else
		        RetStr += _sets.Count.ToString();

	        RetStr += "\n";

	        if (_sets.Count > 0)
	        {
		        foreach (CheesegullBeatmapSet set in _sets)
		        {
			        double MaxDiff = 0;

			        foreach (CheesegullBeatmap cbm in set.ChildrenBeatmaps)
				        if (cbm.DifficultyRating > MaxDiff)
					        MaxDiff = cbm.DifficultyRating;

			        MaxDiff *= 1.5;

			        RetStr += $"{set.SetID}.osz|" +
			                  $"{set.Artist}|" +
			                  $"{set.Title}|" +
			                  $"{set.Creator}|" +
			                  $"{set.RankedStatus}|" +
			                  $"{MaxDiff + ".00"}|" +
			                  $"{set.LastUpdate}|" +
			                  $"{set.SetID}|" +
			                  $"{set.SetID}|" +
			                  $"0|" +
			                  $"1234|" +
			                  $"{Convert.ToInt32(set.HasVideo)}|" +
			                  $"{Convert.ToInt32(set.HasVideo) * 4321}|";

			        foreach (CheesegullBeatmap cb in set.ChildrenBeatmaps)
				        RetStr += $"{cb.DiffName.Replace("@", "")} " +
				                  $"({Math.Round(cb.DifficultyRating, 2)}★~" +
				                  $"{cb.BPM}♫~AR" +
				                  $"{cb.AR}~OD" +
				                  $"{cb.OD}~CS" +
				                  $"{cb.CS}~HP" +
				                  $"{cb.HP}~" +
				                  $"{(int) MathF.Floor(cb.TotalLength) / 60}m" +
				                  $"{cb.TotalLength%60}s)@" +
				                  $"{(int) cb.Mode},";
			        
				    RetStr = RetStr.TrimEnd(',') + "|\n";
		        }
	        }
	        else if (_sets.Count <= 0)
		        RetStr = "-1\nNo Beatmaps were found!";
	        else if (_sets.Count <= 0 && _query == string.Empty)
		        RetStr = "-1\nWhoops, looks like osu!direct is down!";

	        return RetStr;
        }

        public string ToNP()
        {
	        if (_sets.Count <= 0)
		        return "0";

	        CheesegullBeatmapSet set = _sets[0];
	        
	        return $"{set.SetID}.osz|" +
	               $"{set.Artist}|" +
	               $"{set.Title}|" +
	               $"{set.Creator}|" +
	               $"{set.RankedStatus}|" +
	               $"10.00|" +
	               $"{set.LastUpdate}|" +
	               $"{set.SetID}|" +
	               $"{set.SetID}|" +
	               $"{Convert.ToInt32(set.HasVideo)}|" +
	               $"0|" +
	               $"1234|" +
	               $"{Convert.ToInt32(set.HasVideo) * 4321}\r\n";
        }

        public List<CheesegullBeatmapSet> GetSets() => _sets;
    }
}