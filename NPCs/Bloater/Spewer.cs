using Microsoft.Xna.Framework;
using SpiritMod.Items.Sets.EvilBiomeDrops.GastricGusher;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.NPCs.Bloater
{
	public class Spewer : ModNPC
	{
		int frame = 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bloater");
			Main.npcFrameCount[NPC.type] = 9;
			NPCHelper.ImmuneTo(this, BuffID.Poisoned, BuffID.Confused);
		}

		public override void SetDefaults()
		{
			NPC.width = 40;
			NPC.height = 44;
			NPC.damage = 25;
			NPC.defense = 5;
			NPC.knockBackResist = 0.2f;
			NPC.value = 90;
			NPC.lifeMax = 45;
			NPC.HitSound = SoundID.NPCHit18;
			NPC.DeathSound = SoundID.NPCDeath21;
			NPC.noGravity = true;
			NPC.noTileCollide = false;

			Banner = NPC.type;
			BannerItem = ModContent.ItemType<Items.Banners.BloaterBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new FlavorTextBestiaryInfoElement("A particularly foul beast. It builds up pressure by sucking in air with the organs on its back and propels gastric fluids at anything it deems threatening."),
			});
		}

		public override void FindFrame(int frameHeight)
		{
			float distance = NPC.Distance(Main.player[NPC.target].Center);
			if (NPC.ai[1] > 40 && NPC.ai[1] < 180)
			{
				if (distance < 240)
				{
					++NPC.ai[2];
					if (NPC.ai[2] >= 4)
					{
						NPC.ai[2] = 0;
						frame++;
					}
					if (frame >= 9 || frame < 5)
						frame = 7;
				}
				else
				{
					++NPC.ai[2];
					if (NPC.ai[2] >= 10)
					{
						NPC.ai[2] = 0;
						frame++;
					}
					if (frame >= 4)
						frame = 0;
				}
			}
			else
			{
				++NPC.ai[2];
				if (NPC.ai[2] >= 10)
				{
					NPC.ai[2] = 0;
					frame++;
				}
				if (frame >= 4)
					frame = 0;
			}

			NPC.frame.Y = frameHeight * frame;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.ZoneCrimson && spawnInfo.Player.ZoneOverworldHeight && !Main.eclipse ? .075f : 0f;

		public override void AI()
		{
			NPC.rotation = NPC.velocity.X * .08f;
			NPC.TargetClosest(true);
			NPC.ai[1] += 1f;

			float velMax = 1f;
			float acceleration = 0.011f;
			float distance = NPC.Distance(Main.player[NPC.target].Center);

			if (distance < 240)
			{
				if (NPC.ai[1] >= 120 && NPC.ai[1] <= 180)
				{
					if (Main.rand.NextBool(10) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						SoundEngine.PlaySound(SoundID.Item34, NPC.Center);
						Vector2 direction = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center) * new Vector2(11.5f, 8);
						int damage = Main.expertMode ? 11 : 13;
						int vomit = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y + 4, direction.X, direction.Y + Main.rand.NextFloat(-.5f, .5f), ModContent.ProjectileType<VomitProj>(), damage, 1, Main.myPlayer, 0, 0);
						Main.projectile[vomit].netUpdate = true;
						NPC.netUpdate = true;
					}
				}
			}

			if (Main.rand.NextFloat() < 0.131579f)
			{
				int d = Dust.NewDust(NPC.position, NPC.width, NPC.height + 10, DustID.Blood, 0, 1f, 0, new Color(), 0.7f);
				Main.dust[d].velocity *= .1f;
			}

			if (NPC.ai[1] == 120 && distance < 240)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);
				SoundEngine.PlaySound(SoundID.Zombie40, NPC.Center);
			}

			if (NPC.ai[1] > 40 && NPC.ai[1] < 180)
			{
				if (distance < 240)
				{
					float num395 = Main.mouseTextColor / 200f - 0.25f;
					num395 *= 0.2f;
					NPC.scale = num395 + 0.95f;
				}
			}

			if (NPC.ai[1] > 200.0)
			{
				if (NPC.ai[1] > 300.0)
					NPC.ai[1] = 0f;
			}
			else if (distance < 120.0)
			{
				NPC.ai[0] += 0.9f;

				if (NPC.ai[0] > 0f)
					NPC.velocity.Y = NPC.velocity.Y + 0.039f;
				else
					NPC.velocity.Y = NPC.velocity.Y - 0.019f;

				if (NPC.ai[0] < -100f || NPC.ai[0] > 100f)
					NPC.velocity.X = NPC.velocity.X + 0.029f;
				else
					NPC.velocity.X = NPC.velocity.X - 0.029f;

				if (NPC.ai[0] > 25f)
					NPC.ai[0] = -200f;
			}

			if (Main.rand.NextBool(30) && Main.netMode != NetmodeID.MultiplayerClient)
			{
				if (Main.rand.NextBool(2))
					NPC.velocity.Y = NPC.velocity.Y + 0.439f;
				else
					NPC.velocity.Y = NPC.velocity.Y - 0.419f;
				NPC.netUpdate = true;
			}

			if (distance > 350.0)
			{
				velMax = 5f;
				acceleration = 0.2f;
			}
			else if (distance > 300.0)
			{
				velMax = 3f;
				acceleration = 0.25f;
			}
			else if (distance > 250.0)
			{
				velMax = 2.5f;
				acceleration = 0.13f;
			}

			if (distance > 500)
				NPC.noTileCollide = true;
			else
				NPC.noTileCollide = false;

			float deltaX = Main.player[NPC.target].Center.X - NPC.Center.X;
			float deltaY = Main.player[NPC.target].Center.Y - NPC.Center.Y;
			float stepRatio = velMax / distance;
			float velLimitX = deltaX * stepRatio;
			float velLimitY = deltaY * stepRatio;

			if (Main.player[NPC.target].dead)
			{
				velLimitX = (float)((NPC.direction * velMax) / 2.0);
				velLimitY = (float)((-velMax) / 2.0);
			}

			if (NPC.velocity.X < velLimitX)
				NPC.velocity.X = NPC.velocity.X + acceleration;
			else if (NPC.velocity.X > velLimitX)
				NPC.velocity.X = NPC.velocity.X - acceleration;

			if (NPC.velocity.Y < velLimitY)
				NPC.velocity.Y = NPC.velocity.Y + acceleration;
			else if (NPC.velocity.Y > velLimitY)
				NPC.velocity.Y = NPC.velocity.Y - acceleration;
			NPC.spriteDirection = NPC.direction;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 23; k++)
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hitDirection * 1.5f, -1f, 0, default, .91f);

			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath30, NPC.Center);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Spewer1").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Spewer2").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Spewer3").Type, 1f);
			}
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GastricGusher>(), 25));
		}
	}
}