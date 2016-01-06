﻿using Discord.API.Client.Rest;
using System;
using System.IO;
using System.Threading.Tasks;
using APIUser = Discord.API.Client.User;

namespace Discord
{
    public sealed class Profile
    {
        internal DiscordClient Client { get; }

        /// <summary> Gets the unique identifier for this user. </summary>
        public ulong Id { get; }
        /// <summary> Gets the global name of this user. </summary>
        public string Name => Client.PrivateUser.Name;
        /// <summary> Gets the unique identifier for this user's current avatar. </summary>
        public string AvatarId => Client.PrivateUser.AvatarId;
        /// <summary> Gets the URL to this user's current avatar. </summary>
        public string AvatarUrl => Client.PrivateUser.AvatarUrl;
        /// <summary> Gets an id uniquely identifying from others with the same name. </summary>
        public ushort Discriminator => Client.PrivateUser.Discriminator;
        /// <summary> Gets the name of the game this user is currently playing. </summary>
        public string CurrentGame => Client.PrivateUser.CurrentGame;
        /// <summary> Gets the current status for this user. </summary>
        public UserStatus Status => Client.PrivateUser.Status;
        /// <summary> Returns the string used to mention this user. </summary>
        public string Mention => $"<@{Id}>";

        /// <summary> Gets the email for this user. </summary>
        public string Email { get; private set; }
		/// <summary> Gets if the email for this user has been verified. </summary>
		public bool? IsVerified { get; private set; }

        internal Profile(DiscordClient client, ulong id)
        {
            Client = client;
            Id = id;
        }

        internal void Update(APIUser model)
        {
            Email = model.Email;
            IsVerified = model.IsVerified;
        }

        public async Task Edit(string currentPassword = "",
            string username = null, string email = null, string password = null,
            Stream avatar = null, ImageType avatarType = ImageType.Png)
        {
            if (currentPassword == null) throw new ArgumentNullException(nameof(currentPassword));

            var request = new UpdateProfileRequest()
            {
                CurrentPassword = currentPassword,
                Email = email ?? Email,
                Password = password,
                Username = username ?? Client.PrivateUser.Name,
                AvatarBase64 = avatar.Base64(avatarType, Client.PrivateUser.AvatarId)
            };

            await Client.ClientAPI.Send(request).ConfigureAwait(false);

            if (password != null)
            {
                var loginRequest = new LoginRequest()
                {
                    Email = Email,
                    Password = password
                };
                var loginResponse = await Client.ClientAPI.Send(loginRequest).ConfigureAwait(false);
                Client.ClientAPI.Token = loginResponse.Token;
                Client.GatewaySocket.Token = loginResponse.Token;
                Client.GatewaySocket.SessionId = null;
            }
        }

        public override bool Equals(object obj) 
            => (obj is Profile && (obj as Profile).Id == Id) || (obj is User && (obj as User).Id == Id);
        public override int GetHashCode() => unchecked(Id.GetHashCode() + 2061);
        public override string ToString() => Id.ToIdString();
    }
}
