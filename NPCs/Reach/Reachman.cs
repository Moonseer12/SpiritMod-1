using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using SpiritMod.Items.Sets.BriarDrops;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritMod.Items.Consumable.Food;
using Terraria.Audio;
using System.IO;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;
using SpiritMod.Biomes;
using SpiritMod.Utilities;

namespace SpiritMod.NPCs.Reach
{
	public class Reachman : ModNPC
	{
		int frame = 0;
		int aiTimer = 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Feral Shambler");
			Main.npcFrameCount[NPC.type] = 16;
			NPCHelper.ImmuneTo(this, BuffID.Poisoned);

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 36;
			NPC.height = 52;
			NPC.damage = 22;
			NPC.defense = 8;
			NPC.lifeMax = 59;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 70f;
			NPC.knockBackResist = .34f;
			NPC.aiStyle = 3;
			AIType = NPCID.SnowFlinx;
			NPC.HitSound = SoundID.NPCHit2 with { PitchVariance = 0.2f };
			Banner = NPC.type;
            BannerItem = ModContent.ItemType<Items.Banners.ReachmanBanner>();
			SpawnModBiomes = new int[1] { ModContent.GetInstance<BriarSurfaceBiome>().Type };
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("The line between flora and fauna is blurred in the briar. The reanimated bones of adventurers act animalistic, despite it being composed mostly of plants."),
			});
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			Player player = spawnInfo.Player;

			return (spawnInfo.Player.ZoneBriar() && !(player.ZoneTowerSolar || player.ZoneTowerVortex || player.ZoneTowerNebula || player.ZoneTowerStardust) && 
				!(Main.pumpkinMoon || Main.snowMoon || Main.eclipse) && !spawnInfo.Invasion && !spawnInfo.PlayerInTown && SpawnCondition.GoblinArmy.Chance == 0) ? 1.7f : 0f;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(frame);
			writer.Write(aiTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			frame = reader.ReadInt32();
			aiTimer = reader.ReadInt32();
		}

		public override void AI()
		{
			Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0.23f, 0.16f, .05f);
			aiTimer++;

			if (NPC.life <= NPC.lifeMax - 20)
			{
				if (aiTimer == 180)
					SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);

				if (aiTimer > 180 && aiTimer < 360)
				{
					DoDustEffect(NPC.Center, 46f, 1.08f, 2.08f, NPC);
					NPC.velocity = Vector2.Zero;
					if (NPC.velocity == Vector2.Zero)
					{
						NPC.velocity.X = .008f * NPC.direction;
						NPC.velocity.Y = 12f;
					}
				}

				if (aiTimer == 360)
				{
					if (Main.netMode != NetmodeID.Server)
						SoundEngine.PlaySound(new SoundStyle("SpiritMod/Sounds/EnemyHeal"), NPC.Center);
					NPC.life += 10;
					NPC.HealEffect(10, true);
				}
			}
			
			if (aiTimer >= 360)
				aiTimer = 0;
		}

		public void WalkingFrames()
		{
			if (!NPC.collideY && NPC.velocity.Y > 0)
				frame = 0;
			else
			{
				if (NPC.frameCounter >= 4)
				{
					frame++;
					NPC.frameCounter = 0;
				}
				if (frame >= 10)
					frame = 0;
			}
		}

		public void HealingFrames()
		{
			if (NPC.frameCounter >= 4)
			{
				frame++;
				NPC.frameCounter = 0;
			}
			if (frame >= 16 || frame < 10)
				frame = 10;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			if (NPC.life <= NPC.lifeMax - 20)
			{
				if (aiTimer > 180 && aiTimer < 360)
					HealingFrames();
				else
					WalkingFrames();
			}
			else
				WalkingFrames();

			NPC.frame.Y = frameHeight * frame;
		}

		private static void DoDustEffect(Vector2 position, float distance, float minSpeed = 2f, float maxSpeed = 3f, object follow = null)
		{
			float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
			Vector2 vec = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			Vector2 vel = vec * Main.rand.NextFloat(minSpeed, maxSpeed);

			int dust = Dust.NewDust(position - vec * distance, 0, 0, DustID.TreasureSparkle);
			Main.dust[dust].noGravity = true;
			Main.dust[dust].scale *= .6f;
			Main.dust[dust].velocity = vel;
			Main.dust[dust].customData = follow;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.AddCommon<SanctifiedStabber>(20);
			npcLoot.AddFood<CaesarSalad>(33);

			LeadingConditionRule notDay = new LeadingConditionRule(new DropRuleConditions.NotDay());
			notDay.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EnchantedLeaf>()));
			npcLoot.Add(notDay);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.rand.NextBool(10) && Main.expertMode)
				target.AddBuff(148, 2000);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => GlowmaskUtils.DrawNPCGlowMask(spriteBatch, NPC, Mod.Assets.Request<Texture2D>("NPCs/Reach/Reachman_Glow").Value, screenPos);

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 30; k++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, 2.5f * hitDirection, -2.5f, 0, Color.White, 0.7f);
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, 2.5f * hitDirection, -2.5f, 0, default, .34f);
			}

			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Reach1").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Reach2").Type, 1f);
			}
		}
	}
}
