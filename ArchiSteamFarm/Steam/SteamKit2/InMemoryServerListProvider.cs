// ----------------------------------------------------------------------------------------------
//     _                _      _  ____   _                           _____
//    / \    _ __  ___ | |__  (_)/ ___| | |_  ___   __ _  _ __ ___  |  ___|__ _  _ __  _ __ ___
//   / _ \  | '__|/ __|| '_ \ | |\___ \ | __|/ _ \ / _` || '_ ` _ \ | |_  / _` || '__|| '_ ` _ \
//  / ___ \ | |  | (__ | | | || | ___) || |_|  __/| (_| || | | | | ||  _|| (_| || |   | | | | | |
// /_/   \_\|_|   \___||_| |_||_||____/  \__|\___| \__,_||_| |_| |_||_|   \__,_||_|   |_| |_| |_|
// ----------------------------------------------------------------------------------------------
// |
// Copyright 2015-2025 Łukasz "JustArchi" Domeradzki
// Contact: JustArchi@JustArchi.net
// |
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// |
// http://www.apache.org/licenses/LICENSE-2.0
// |
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArchiSteamFarm.Collections;
using ArchiSteamFarm.Helpers.Json;
using JetBrains.Annotations;
using SteamKit2.Discovery;

namespace ArchiSteamFarm.Steam.SteamKit2;

internal sealed class InMemoryServerListProvider : IServerListProvider {
	[JsonInclude]
	public DateTime LastServerListRefresh { get; private set; }

	[JsonDisallowNull]
	[JsonInclude]
	private ConcurrentList<ServerRecordEndPoint> ServerRecords { get; init; } = [];

	public Task<IEnumerable<ServerRecord>> FetchServerListAsync() => Task.FromResult(ServerRecords.Where(static server => !string.IsNullOrEmpty(server.Host) && server is { Port: > 0, ProtocolTypes: > 0 }).Select(static server => ServerRecord.CreateServer(server.Host, server.Port, server.ProtocolTypes)));

	public Task UpdateServerListAsync(IEnumerable<ServerRecord> endpoints) {
		ArgumentNullException.ThrowIfNull(endpoints);

		LastServerListRefresh = DateTime.UtcNow;

		IEnumerable<ServerRecordEndPoint> serverRecords = endpoints.Select(static endpoint => new ServerRecordEndPoint(endpoint.GetHost(), (ushort) endpoint.GetPort(), endpoint.ProtocolTypes));

		ServerRecords.ReplaceWith(serverRecords);

		ServerListUpdated?.Invoke(this, EventArgs.Empty);

		return Task.CompletedTask;
	}

	[UsedImplicitly]
	public bool ShouldSerializeServerRecords() => ServerRecords.Count > 0;

	internal event EventHandler? ServerListUpdated;
}
