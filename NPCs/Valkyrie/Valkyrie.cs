using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Items.Weapon.Magic;
using SpiritMod.Mechanics.QuestSystem;
using SpiritMod.Mechanics.QuestSystem.Quests;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.NPCs.Valkyrie
{
	public class Valkyrie : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Valkyrie");
			Main.npcFrameCount[NPC.type] = 8;
			NPCID.Sets.TrailCacheLength[NPC.type] = 3;
			NPCID.Sets.TrailingMode[NPC.type] = 0;
		}

		public override void SetDefaults()
		{
			NPC.width = 88;
			NPC.height = 60;
			NPC.damage = 20;
			NPC.defense = 15;
			NPC.lifeMax = 120;
			NPC.noGravity = true;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 360f;
			NPC.rarity = 2;
			NPC.knockBackResist = .45f;
			NPC.aiStyle = 14;
			Banner = NPC.type;
			BannerItem = ModContent.ItemType<Items.Banners.ValkyrieBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,
				new FlavorTextBestiaryInfoElement("Righteous defenders of the sky, these winged warriors harness the power of the sun to protect the islands that float above the land."),
			});
		}

		int aiTimer;
		bool trailing;

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.AddCommon(ItemID.Feather, 1, 2, 3);
			npcLoot.AddCommon(ItemID.GiantHarpyFeather, 100);
			npcLoot.AddCommon<ValkyrieSpear>(10);
		}

		public override void AI()
		{
			aiTimer++;
			if (aiTimer == 100 || aiTimer == 480)
			{
				SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, NPC.Center);

				var direction = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center) * Main.rand.Next(6, 9);
				NPC.velocity = direction * 0.98f;
			}

			trailing = aiTimer >= 100 && aiTimer <= 120 || aiTimer >= 480 && aiTimer <= 500;

			if (aiTimer >= 120 && aiTimer <= 300)
			{
				int dust = Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.PortalBolt);
				Main.dust[dust].velocity *= -1f;
				Main.dust[dust].noGravity = true;

				Vector2 dustSpeed = Vector2.Normalize(new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101)));
				dustSpeed *= (Main.rand.Next(50, 100) * 0.04f);
				Main.dust[dust].velocity = dustSpeed;
				Main.dust[dust].position = NPC.Center - Vector2.Normalize(dustSpeed) * 34f;
			}

			if (aiTimer == 300)
			{
				SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 direction = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center) * 9f;
					int damage = Main.expertMode ? 9 : 15;

					int amountOfProjectiles = Main.rand.Next(2, 4);
					for (int i = 0; i < amountOfProjectiles; ++i)
					{
						float A = Main.rand.Next(-150, 150) * 0.01f;
						float B = Main.rand.Next(-150, 150) * 0.01f;
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, direction.X + A, direction.Y + B, ModContent.ProjectileType<ValkyrieSpearHostile>(), damage, 1, Main.myPlayer, 0, 0);
					}
				}
			}

			if (aiTimer >= 500)
				aiTimer = 0;
		}

		public override void FindFrame(int frameHeight)
		{
			if (aiTimer == 270)
				NPC.frameCounter = 0;

			if (aiTimer < 270 || aiTimer > 330)
			{
				NPC.frameCounter += 0.15f;
				NPC.frameCounter %= 4;
				int frame = (int)NPC.frameCounter;
				NPC.frame.Y = frame * frameHeight;
			}
			else
			{
				NPC.frameCounter += 0.0666f;
				NPC.frameCounter %= 4;
				int frame = (int)NPC.frameCounter + 4;
				NPC.frame.Y = frame * frameHeight;
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (QuestManager.GetQuest<SlayerQuestValkyrie>().IsActive && !spawnInfo.PlayerInTown)
				return 0.15f;

			return spawnInfo.Sky && !spawnInfo.PlayerInTown && !NPC.AnyNPCs(ModContent.NPCType<Valkyrie>()) ? 0.09f : 0;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Valkyrie1").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Valkyrie1").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Valkyrie2").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Valkyrie3").Type, 1f);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 origin = NPC.frame.Size() / 2;
			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			drawColor = (Main.LocalPlayer.detectCreature && !NPC.IsABestiaryIconDummy) ? new Color(255, 50, 50) : Color.White;

			if (trailing)
			{
				for (int k = 0; k < NPC.oldPos.Length; k++)
				{
					Vector2 drawPos = NPC.oldPos[k] - screenPos + (NPC.Size / 2) + new Vector2(0f, NPC.gfxOffY);
					Color trailColor = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length / 2);
					spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, trailColor, NPC.rotation, origin, NPC.scale, effects, 0f);
				}
			}

			Color color = NPC.GetNPCColorTintedByBuffs(drawColor);
			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame, NPC.GetAlpha(color), NPC.rotation, origin, NPC.scale, effects, 0);

			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			//var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			//var color = (Main.LocalPlayer.detectCreature && !NPC.IsABestiaryIconDummy) ? new Color(255, 50, 50) : Color.White;
			//spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame, color, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, effects, 0f);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.rand.NextBool(5))
				target.AddBuff(BuffID.Bleeding, 300);
		}
	}
}