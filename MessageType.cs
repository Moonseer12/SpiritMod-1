﻿namespace SpiritMod
{
	public enum MessageType : byte
	{
		None = 0,
		AuroraData,
		ProjectileData,
		PlaceMapPin,
		Dodge,
		Dash,
		PlayerGlyph,
		BossSpawnFromClient,
		SpawnGladeWraith,
		SpawnDebris,
		StartTide,
		TideData,
		TameAuroraStag,
		SpawnTrail,
		PlaceSuperSunFlower,
		DestroySuperSunFlower,
		SpawnExplosiveBarrel,
		SpawnStardustBomb,
		StarjinxData,
		BoonData,
		FathomlessData,
		RequestQuestManager,
		RecieveQuestManager,
		Quest,
	}

	public enum QuestMessageType : byte
	{
		Deactivate = 0,
		Activate,
		ProgressOrComplete,
		SyncOnNPCLoot,
		SyncOnEditSpawnPool,
		ObtainQuestBook,
		Unlock,
		SyncNPCQueue
	}
}
