using System.Collections.Generic;
using Newtonsoft.Json;

namespace Slack2FAChecker
{
	public class UserListResponse
	{
		[JsonProperty("ok")]
		public bool Ok { get; set; }

		[JsonProperty("members")]
		public IList<Member> Members { get; set; }

		[JsonProperty("cache_ts")]
		public long CachedAt { get; set; }

		[JsonProperty("response_metadata")]
		public ResponseMetadata ResponseMetadata { get; set; }
	}

	public class ResponseMetadata
	{
		[JsonProperty("next_cursor")]
		public string NextCursor { get; set; }
	}

	public class Member
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("team_id")]
		public string TeamId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("deleted")]
		public bool Deleted { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }

		[JsonProperty("real_name")]
		public string RealName { get; set; }

		[JsonProperty("tz")]
		public string TimeZone { get; set; }

		[JsonProperty("tz_label")]
		public string TimeZoneLabel { get; set; }

		[JsonProperty("tz_offset")]
		public long TimeZoneOffset { get; set; }

		[JsonProperty("profile")]
		public Profile Profile { get; set; }

		[JsonProperty("is_admin")]
		public bool IsAdmin { get; set; }

		[JsonProperty("is_owner")]
		public bool IsOwner { get; set; }

		[JsonProperty("updated")]
		public long UpdatedAt { get; set; }

		[JsonProperty("has_2fa")]
		public bool Has2Fa { get; set; }
	}

	public class Profile
	{
		[JsonProperty("avatar_hash")]
		public string AvatarHash { get; set; }

		[JsonProperty("current_status")]
		public string CurrentStatus { get; set; }

		[JsonProperty("first_name")]
		public string FirstName { get; set; }

		[JsonProperty("last_name")]
		public string LastName { get; set; }

		[JsonProperty("real_name")]
		public string RealName { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("skype")]
		public string Skype { get; set; }

		[JsonProperty("phone")]
		public string Phone { get; set; }

		[JsonProperty("image_24")]
		public string Image24 { get; set; }

		[JsonProperty("image_32")]
		public string Image32 { get; set; }

		[JsonProperty("image_48")]
		public string Image48 { get; set; }

		[JsonProperty("image_72")]
		public string Image72 { get; set; }

		[JsonProperty("image_192")]
		public string Image192 { get; set; }
	}
}